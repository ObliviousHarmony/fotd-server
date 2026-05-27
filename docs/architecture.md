# Architecture

## Project Directory Structure

Each of the three .NET projects follow [Clean Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
and have their project directories organized as such.

## [FOMNetwork](../fom-network)

Face of Mankind uses RakNet 3.611 for client/server communication. Rather than
exposing it to managed code directly, this native library provides consumers
with structured packet objects that they can process. It is responsible for
serialization, deserialization, and the management of network connections.
The API focuses on minimizing calls across the interop boundary and emphasizes
the use of [blittable types](https://learn.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types)
to avoid data marshalling between managed and unmanaged code.

### Packet Structs and Serialization

All packet types are represented with dedicated blittable structs. Each struct is sized to
accommodate the maximum data a packet can contain, using fixed-size arrays for variable-length
fields with a count field to indicate how many entries are valid. This approach eliminates
per-packet heap allocations and enables zero-copy interop at the cost of some memory overhead.

RakNet uses `BitStream` for compact network serialization with support for compression and
variable-length encoding. The native library handles all `BitStream` operations through dedicated
reader/writer serializers for each packet type. This allows managed code to work with clean,
structured data without needing to understand the wire format or serialization details.

### Interop Struct Validation

One of the important things to remember when dealing with blittable types is
that they work by passing pointers to blocks of memory across the interop
boundary. This only works as long as both the native and managed structures
are identical down to the bit. They must share the same overall size and
the containing fields must be the same size and have the same offsets. Failing
to meet this criteria will lead to undefined behavior since the same sections
of memory will be interpreted differently between native and managed code.

In order to make sure this happens the library supplies [a validation function](../fom-network/include/fom-network/NetworkApi.h).
Consumers are expected to call this and provide all of the structures used for the packet data.

### Connection Management

The library supports both [listening for connections](../fom-network/include/fom-network/ServerApi.h)
and [opening them with remote hosts](../fom-network/include/fom-network/ClientApi.h).
Consumers are provided `RakPeerInterface` pointers that are then used by the
rest of the library's functions to interact with the network. Since these are
a blittable type, managed code is able to hold on to them without marshalling.

**Please note that RakNet _requires_ each peer to be used only by a single thread at a time.
You must either lock peer usage or have it be owned by a single thread.**

### Packet Receiving

Rather than having the consumer deal with the packet bitstream directly, the library takes care
of deserializing them into a structured format with named fields. This means consumers don't
need to know anything about the raw packets themselves and get data back that is easily
consumed.

In an effort to avoid excessive heap allocations, packet receiving and processing takes
place in two stages:

- **Receiving** (`FOMNetwork_ReceivePackets`): The consumer calls into the library to poll RakNet for packets.
Instead of immediately parsing them, the library returns metadata about the received packets
(packet identifiers, sender addresses, and count). This allows managed code to determine
how much buffer space is needed.

- **Processing** (`FOMNetwork_ProcessPackets`): Using the metadata from the receiving step, the consumer
provides a buffer of managed memory sized appropriately for the packets. The library then
deserializes the packet BitStreams directly into this buffer, creating structured packet
objects that the consumer can handle.

Each packet slot in the buffer uses the format: `[status byte][packet data...]`. The status byte
indicates whether deserialization succeeded (`SERIALIZATION_SUCCESS = 0`) or failed
(`SERIALIZATION_READ_ERROR = 1`, `SERIALIZATION_UNHANDLED_PACKET = 2`). This means the buffer
size must be calculated as the sum of `1 + sizeof(packet)` for each packet received.

This two-stage approach enables managed code to control memory allocation and use pooled
buffers, avoiding per-packet heap allocations.

## [ServerShared](../server-shared)

This class library serves as the central framework for both the Master and World server. It
takes care of packet processing, logging, and supplies tools for database persistence. The
library also contains all of the packet structures.

### Packet Flow Overview

Packets flow through the system from the network to handlers:

1. **Network Thread** polls RakNet and receives packet metadata
2. **PacketBuffer Pool** provides a reusable buffer for deserialization
3. **PacketRef Distribution** creates type-safe references with automatic cleanup
4. **Processor Queue** receives the references for async processing
5. **Worker Threads** dispatch packets to their registered handlers

This design achieves zero-copy packet handling while supporting concurrent processing
through reference counting and buffer pooling.

### Packet Buffers and References

The `PacketBuffer` class manages pooled memory for batches of received packets. When packets arrive:

1. A buffer is rented from the pool (or created if none available)
2. Native code deserializes packets directly into this buffer (with status byte prefix)
3. `PacketRef` structs are created, each holding a `Memory<byte>` slice pointing to a specific packet
4. Each ref holds a reference to its parent buffer
5. When a ref is disposed, it decrements the buffer's ref count
6. When all refs are disposed, the buffer becomes available for reuse

Each `PacketRef` exposes a `Status` property that reads the status byte from the buffer. Packets
with non-success status are logged and discarded by `NetworkManager` before reaching the processor.
Attempting to call `Data<T>()` on a packet with a failed status throws `InvalidOperationException`.

This reference-counting approach allows packets to be processed concurrently by multiple
threads while ensuring buffers are safely returned to the pool.

### Packet Sending

Sending packets uses the `PacketWriter<TPacket>` struct, which manages buffer pooling automatically:

1. Handlers create a `PacketWriter<TPacket>` which rents a buffer from `ArrayPool<byte>.Shared`
2. The handler populates packet fields via the `Data` property (a `ref` to the packet struct)
3. Destination addresses are added via `AddDestination()`, or `ExcludeFromBroadcast()` for broadcasts
4. Calling `Build()` transfers buffer ownership to a `QueuePacket` and marks the writer as consumed
5. The packet is sent through an intermediary sender which queues it for the network thread
6. The network thread batches packets, copies them into a contiguous pinned buffer
7. All packets in the batch are sent to the networking library in one call
8. Buffers are returned to their respective pools

By default, a `PacketWriter` is configured for broadcast mode. To send to specific addresses,
call `AddDestination()` which switches the packet to direct mode.

Example usage in a handler:

```csharp
using var response = new PacketWriter<LoginReturn>();
ref var data = ref response.Data;

data.Status = LoginReturn.StatusCode.LOGIN_RETURN_SUCCESS;
data.PlayerId = player.Id;

response.AddDestination(sender);
_packetSender.Send(response.Build());
```

The `using` declaration ensures the buffer is returned to the pool if `Build()` is never called
(e.g., due to an early return or exception). Once `Build()` is called, ownership transfers to
the `QueuePacket` and the writer no longer manages the buffer.

### Threading Model

Each server makes use of a number of different threads to isolate the packet handlers from
expensive IO. This maximizes throughput so that it can handle them as quickly as possible.

- **Network Thread**: RakNet requires that peer-related function calls be confined to a
  single thread. This thread is responsible for both receiving and sending packets. Received
  packets are enqueued to the packet processor, and packets to send are read from a channel
  and batched for transmission. The thread uses exponential backoff when idle to reduce CPU
  usage while remaining responsive.

- **Packet Handler Threads**: Once a packet has been received it is enqueued to a channel
  to be handled by one of several packet handling threads. This allows concurrent packet
  processing, maximizing throughput. Handlers may process packets out of order, but this
  is acceptable for most game logic.

- **Update Thread**: Using `PeriodicTimer`, this thread runs periodic operations based on
  elapsed time and performs tasks not directly related to packet handling (e.g., timed events,
  cleanup, scheduled actions).

- **Logging Thread**: In general, logging is an operation that makes use of blocking IO.
  A dedicated thread with a lock-free queue allows for asynchronous logging that does
  not block other threads.

- **Persistence Thread**: One of the most expensive operations that the server can
  perform is synchronizing state to the database. With the aim of eventual consistency,
  changes to state in memory can be queued to be persisted asynchronously. This keeps
  packet handlers from having to do expensive database IO unless strictly required
  by the behavior of the handler.

### Packet Handlers

The class library provides a framework for making it easy to handle packets. Handlers extend
`BasePacketHandler<T>` where `T` is the specific packet struct they process. Once registered
with the DI container (via the `[PacketHandler]` attribute), they are automatically discovered
and added to the processing pipeline.

See the [packet handler guide](adding-packet-handlers.md) for detailed instructions on creating
new packet types and handlers.

### Packet Routing

Multiple `NetworkManager` instances can coexist (e.g., one for client connections, one for
server-to-server communication). To prevent clients from spoofing internal packets, network
managers can "claim" packet IDs using `ClaimPacketId()`. When a packet with a claimed ID
is received by a different network manager, it is ignored and logged.

### Database Persistence

An interface is provided to mark a class as persistable. Once registered with the
persistence service, changes logged through the interface will queue the instance
for database synchronization. A dedicated handler then takes the instance and
serializes it to the database. Since this process is asynchronous, it allows for
changes to be made in-memory and relies on them being eventually written to the
database.

## [MasterServer](../master-server)

This server is responsible for any state and behavior that is not associated with any single world.
It manages player authentication, character creation, factions, World Servers, and more.

The Master Server runs two separate `NetworkManager` instances:
- **Client Network**: Handles connections from game clients
- **World Network**: Manages connections to World Servers

## [WorldServer](../world-server/)

This server is responsible for any state and behavior that relates to a given world
and the players that are interacting on it. Each World Server handles game simulation,
player movement, combat, and other in-world interactions.

The World Server runs two separate `NetworkManager` instances:
- **Client Network**: Handles connections from game clients
- **Master Network**: Maintains a client connection to the Master Server
