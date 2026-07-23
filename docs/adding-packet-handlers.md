# Adding Packet Handlers

Each packet must exist in both the native library and the shared server
library in order to be received and processed. There are a few steps that
must be done in order to achieve that.

## [FOMNetwork](/fom-network/include/fom-network/enums/PacketIdentifier.h)

- [ ] **Packet Data**: Each packet requires a struct defining what it
  contains. Create a file in [`include/fom-network/packets/`](/fom-network/include/fom-network/packets/)
  named after the packet to be added. Limit data types used to RakNet's provided ones with
  `_t` suffixes to ensure compatibility with the managed structs.

```cpp
#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ExamplePacket {
  uint8_t exampleString[19];
  uint16_t exampleShort;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ExamplePacket);

}  // namespace FOMNetwork
```

- [ ] **Serializer Declaration**: Each packet to be sent or received
  [requires a reader and/or writer](/fom-network/src/packets/PacketSerializers.h)
  that is able to deal with the packet's `BitStream`. Macros are provided to eliminate
  the boilerplate associated with adding a new packet serializer.

```cpp
SERIALIZER_WRITE(ExamplePacket)
// or
SERIALIZER_READ(ExamplePacket)
// or
SERIALIZER_BOTH(ExamplePacket)
```

- [ ] **Serializer Definition**: Based on the macro used, each packet is required to have
  a write and/or a read method defined for the serializer. This is done by creating a new file
  in [`src/packets/`](/fom-network/src/packets/) named `<PacketName>Serializer.cpp`.

```cpp
#include <fom-network/packets/ExamplePacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

void ExamplePacketSerializer::Write(RakNet::BitStream& bs,
                                    const ExamplePacket* data) const {
  EncodeString(bs, data->exampleString);
  bs.WriteCompressed(data->exampleShort);
}

bool ExamplePacketSerializer::Read(RakNet::BitStream& bs,
                                   ExamplePacket* data) const {
  if (!DecodeString(bs, data->exampleString)) return false;
  if (!bs.ReadCompressed(data->exampleShort)) return false;
  return true;
}

}  // namespace FOMNetwork
```

> [!NOTE]
> Read methods must check the return value of each read operation and return `false` on failure.
> This sets the packet's status byte to `SERIALIZATION_READ_ERROR`, allowing managed code to
> detect and handle malformed packets gracefully.

- [ ] **Enable Packet + Serializer**: Each packet size and serializer needs to be
  [included in a map](/fom-network/src/FOMDataSerializer.cpp) in order for the network
  library to make use of it when reading and writing packet data. It should only be
  added to the map that corresponds with the behavior that it implements.

```cpp
static const std::unordered_map<uint8_t, size_t> packetSizes = {
    ...
    {Enum::ID_EXAMPLE, sizeof(ExamplePacket)},
};

static const std::unordered_map<uint32_t, IWriter*> writerMap = {
    ...
    {Enum::ID_EXAMPLE, &ExamplePacketSerializer::GetInstance()},
};

static const std::unordered_map<uint32_t, IReader*> readerMap = {
    ...
    {Enum::ID_EXAMPLE, &ExamplePacketSerializer::GetInstance()},
};
```

- [ ] **CMakeLists.txt**: Add the new packet header and serializer source file to
  [`CMakeLists.txt`](/fom-network/CMakeLists.txt). Headers go in `FOMNETWORK_INC` and
  source files go in `FOMNETWORK_SRC`.

```cmake
set(FOMNETWORK_SRC
  ...
  "src/packets/ExamplePacketSerializer.cpp"
)
set(FOMNETWORK_INC
  ...
  "include/fom-network/packets/ExamplePacket.h"
)
```

## [ServerShared](/server-shared/Interop/FOMNetwork/Enums/PacketIdentifier.cs)

- [ ] **Packet Data**: Each native packet data struct requires a mirror copy [in the
  interop contracts module](/server-shared/Interop/FOMNetwork/Packets/).

> [!CAUTION]
> Both the data types and field order must be identical!

```csharp
using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    // Each packet must be given a PacketId attribute.
    // This hooks the struct up and ensures that it
    // is handled correctly and validated in the
    // places where it needs to be.
    [PacketId(PacketIdentifier.ID_EXAMPLE)]
    // This attribute ensures that the field layout is not changed.
    // Note that the "Pack = 1" option mirrors the
    // `#pragma pack(push, 1)` in the native struct.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ExamplePacket
    {
        // "fixed byte" arrays are required for strings to be blittable.
        public fixed byte ExampleString[19];
        public ushort ExampleShort;
    }
}
```

## Master/World Server

Although all of the packets are defined in the shared class library, each type will generally only be handled
by one of the server types. Each packet handler is defined in their respective server projects.

- [ ] **Packet Handler**: Each handler should be created in their own
  `Application/Handlers/<PacketType>Handler.cs` file.

```csharp
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;

namespace FOMServer.<Master|World>.Application.Handlers
{
    // Each handler must be given the PacketHandler attribute
    // to register it with the packet handling system.
    [PacketHandler]
    public class ExamplePacketHandler : PacketHandlerBase<ExamplePacket>
    {
        public override void Handle(NetworkAddress sender, in ExamplePacket data) { }
    }
}
```

- [ ] **Claim Server<->Server Packets**: If the packet is intended to be sent from one server to another,
      it must be claimed by the correct network manager to prevent it from being sent by a game client.
      This is done in the server's `Server.cs` file after the network manager is created.

```csharp
networkManager.ClaimPacketId(PacketIdentifier.ID_EXAMPLE);
```

By default, other network managers log a warning when they receive a claimed packet, which helps
catch clients spoofing internal packets. For packets legitimately expected on more than one manager
(such as RakNet connection packets), pass `NetworkManager.PacketClaimBehavior.IgnoreSilently` to
suppress that warning.
