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

### Packet Structs

To simplify the interop there is a central `FOMPacket` struct. This contains a discriminated
union of all the types of packet data that the game sends or receives. This allows consumers
to have a single struct instead of needing to work with complicated variable length buffers.
A union like this _does_ use more memory (it allocates space for the largest struct in the
union), however, the packets are short-lived enough that it doesn't use very much memory.

### Interop Struct Validation

One of the important things to remember when dealing with blittable types is
that they work by passing pointers to blocks of memory across the interop
boundary. This only works as long as both the native and managed structures
are indentical down to the bit. They must share the same overall size and
the containing fields must be the same size and have the same offsets. Failing
to meet this criteria will lead to undefined behavior since the same sections
of memory will be interpreted differently between native and managed code.

In order to make sure this happens the library supplies [a validation function](../fom-network/include/fom-network/NetworkAPI.h).
Consumers are expected to call this and provide all of the structures used for the packet data.

### Connection Management

The library supports both [listening for connections](../fom-network/include/fom-network/ServerAPI.h)
and [opening them with remote hosts](../fom-network/include/fom-network/ClientAPI.h).
Consumers are provided `RakNetInterface` pointers that are then used by the
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
place in two stages.

- **Receiving**: The consumer calls into the library to poll RakNet for packets.
Instead of immediately parsing them and returning them, the library returns a structure that
contains all of the packet pointers and a count.
- **Processing**: Using the count from the receiving step, the consumer provides a buffer of
managed memory that the library can write to. The packet pointer structure from the previous
step is passed back with the buffer and the library deserializes the packet bitstreams into
structured packet objects that the consumer can handle.

The server takes advantage of this two-stage process by using the same buffer every time it
calls into the native library to receive packets. This is important because packets are
created and destroyed almost instantly and would generate a significant amount of garbage.
The server also only passes the struct around by reference, thus avoiding expensive copies
of the struct too.

This is why it is **vital** for the packet structures to **all** be blittable.

## [ServerShared](../server-shared)

This class library serves as the central framework for both the Master and World server. It
takes care of packet processing, logging, and supplies tools for database persistence. The
library also contains all of the packet structures.

### Packet Structs

As noted, the `FOMPacket` contains a discriminated union of all packet data types. Although
not every packet type is used by both servers, in order to match the native struct definition,
all of the packet types and the union must be declared in the class library.

### Threading Model

Each server makes use of a number of different threads to isolate the packet handlers from
expensive IO. This maximizes throughput so that it can handle them as quickly as possible.

- **Logging Thread**: In general, logging is an operation that makes use of blocking IO.
  A dedicated thread with a lock-free queue allows for asynchronous logging that does
  not block other threads.
- **Network Thread**: RakNet requires that peer-related function calls be confined to a
  single thread. This thread is responsible for receiving and sending packets from the
  peer that it was initialized with.
- **Packet Handler Threads**: Once a packet has been received it is dropped into a
  lock-free queue to be handled by one of a number of packet handling threads.
- **Persistence Thread**: One of the most expensive operations that the server can
  perform is synchronizing state to the database. With the aim of eventual consistency,
  changes to state in memory can be queued to be persisted asynchronously. This keeps
  packet handlers from having to do expensive database IO unless strictly required
  by the behavior of the handler.

### Packet Handlers

The class library provides a framework for making it easy to handle packets. A base class for
packet handlers is provided that unwraps the raw packet into the specific data struct that
the handler is interested in. Once the consumer's packet handlers are registered with the DI
container they will automatically be added to the processing pipeline and will be handed
packets of the defined type.

### Database Persistence

An interface is provided to mark a class as persistable. Once registered with the
persistence service, changes logged through the interface will queue the instance
for database synchronization. A dedicated handler then takes the instance and
serializes it to the database. Since this process is asynchronous, it allows for
changes to be made in-memory and relies on them being eventually written to the
database.

## [MasterServer](../master-server)

This server is responsible for any state and behavior that is not associated with any single world.

## [WorldServer](../world-server/)

This server is responsible for any state and behavior that relates to a given world
and the players that are interacting on it.
