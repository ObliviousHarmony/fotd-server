# Adding Packet Handlers

Each packet must exist in both the native library and the shared server
library in order to be received and processed. There are a few steps that
must be done in order to achieve that.

## [FOMNetwork](../fom-network/include/fom-network/PacketIdentifier.h)

> [!CAUTION]
> Please note that _all_ files added to the FOMNetwork shared library _must_ be added
> to the list of files in order to be built. This is done using the
> [`CMakeLists.txt`](../fom-network/CMakeLists.txt) file. Failure to add these files
> correctly will result in compiler errors.

- [ ] **Packet Data**: Each packet requires a struct defining what it
  contains. Create a file in [`include/fom-network/packets/data/`](../fom-network/include/fom-network/packets/data/)
  named after the packet to be added. Limit data types used to RakNet's provided ones with
  `_t` suffixes to ensure compatibility with the managed structs.

```cpp
#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {

#pragma pack(push, 1) // MUST PACK BY 1 FOR CONSISTENCY WITH C#
struct ExamplePacket {
  uint8_t exampleString[19];
  uint16_t exampleShort;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ExamplePacket);

}  // namespace FOMNetwork
```

- [ ] **Serializer Declaration**: Each packet to be sent or received
  [requires a reader and/or writer](../fom-network/include/fom-network/packets/PacketSerializers.h)
  that is able to deal with the packet's `BitStream`. Macros are provided to eliminate
  the boilerplate associated with adding a new packet serializer.

```cpp
// The first argument is the name of the packet struct and
// the second is the name of the union variable used to
// access the data contained within the data struct.
SERIALIZER_WRITE(ExamplePacket, examplePacket)
// or
SERIALIZER_READ(ExamplePacket, examplePacket)
// or
SERIALIZER_BOTH(ExamplePacket, examplePacket)
```

- [ ] **Serializer Definition**: Based on the macro used, each packet is required to have
  a write and/or a read method defined for the serializer. This is done by creating a new file
  in [`src/packet-serializers/`](../fom-network/src/packet-serializers/data/) named `<PacketStruct>Serializer`.

```cpp
#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

void ExamplePacketSerializer::WriteData(
  RakNet::BitStream& bs, const Packet::ExamplePacket& data) const {
  EncodeString(bs, data.exampleString);
  bs.WriteCompressed(data.exampleShort);
}

bool ExamplePacketSerializer::ReadData(RakNet::BitStream& bs, Packet::ExamplePacket& data) const {
  DecodeString(bs, data.exampleString);
  bs.ReadCompressed(data.exampleShort);
  return true;
}

}  // namespace FOMNetwork
```

- [ ] **Enable Packet + Serializer**: Each packet size and serializer needs to be
  [included in a map](../fom-network/src/FOMDataSerializer.cpp) in order for the network
  library to make use of it when reading and writing packet data. It should only be
  added to the map that corresponds with the behavior that it implements.

```cpp
const std::unordered_map<uint8_t, size_t> FOMDataSerializer::PacketSizes = {
    ...
    {ID_EXAMPLE, sizeof(Packet::ExamplePacket)},
};
static std::unordered_map<uint32_t, IWriter*> writerMap = {
    ...
    {ID_EXAMPLE, &ExamplePacketSerializer::GetInstance()}};
static std::unordered_map<uint32_t, IReader*> readerMap = {
    ...
    {ID_EXAMPLE, &ExamplePacketSerializer::GetInstance()},
};
```

## [ServerShared](../server-shared/Core/Enums/PacketIdentifier.cs)

- [ ] **Packet Data**: Each native packet data struct requires a mirror copy [in the
  shared server class library](../server-shared/Core/FOMPacket/Data).

> [!CAUTION]
> Both the data types and field order must be identical!

```csharp
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    // Each packet must be given a PacketID attribute.
    // This hooks the struct up and ensures that it
    // is handled correctly and validated in the
    // places where it needs to be.
    [PacketID(PacketIdentifier.ID_EXAMPLE)]
    // This attribute ensures that the field layout is not changed.
    // Note that the "Pack = 1" option mirrors the
    // `#pragma pack(push, 1)` in the native struct.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe public struct ExamplePacket
    {
        // "fixed byte" arrays are required for strings to be blittable.
        public fixed byte ExampleString[19];
        public ushort ExampleShort;
    }
}
```

## Master/World Server

Although all of the packets are defined in the shared class library, each type will generally only be handled
bye one of the server types. Each packet handler is defined in their respective server projects.

- [ ] **Packet Handler**: Each handler should be created in their own
  `Application/Handlers/<PacketType>Handler.cs` file.

```csharp
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Application.Handlers
{
    // Each handler must be given the PacketHandler attribute 
    // to register it with the packet handling system.
    [PacketHandler]
    public class ExamplePacketHandler : PacketHandlerBase<ReadPacketError>
    {
        public override void Handle(NetworkAddress sender, in ExamplePacket data) { }
    }
}
```

- [ ] **Claim Server<->Server Packets**: If the packet is intended to be sent from one server to another,
      it must be claimed by the correct network manager to prevent it from being sent by a game client.
      This is done in the server's `Server.cs` file after the network manager is created.

```csharp
networkManager.ClaimPacketID(PacketIdentifier.ID_EXAMPLE);
```
