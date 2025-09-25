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
  contains. Create a file in [`include/fom-network/packets/`](../fom-network/include/fom-network/packets/)
  named after the packet to be added. Limit data types used to RakNet's provided ones with
  `_t` suffixes to ensure compatibility with the managed structs.

```cpp
#pragma once

#include <fom-network/PacketIdentifier.h>

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

namespace FOMPacket {
struct ExamplePacket {
  uint8_t exampleString[19];
  uint16_t exampleShort;
};
ASSERT_BLITTABLE(ExamplePacket);
}  // namespace FOMPacket

#pragma pack(pop)
```

- [ ] **Data Union**: Each packet needs to be included in [the `FOMDataUnion`
  union](../fom-network/include/fom-network/FOMPacket.h) in order to be seen by the server.

```cpp
/**
 * A union representing all of FoM's packet data types.
 */
struct FOMDataUnion {
  union {
    ...
    FOMPacket::ExamplePacket examplePacket;
  };
};
```

- [ ] **Serializer Declaration**: Each packet to be sent or received
  [requires a reader and/or writer](../fom-network/include/fom-network/PacketSerializers.h)
  that is able to deal with the packet's `BitStream`. Macros are provided to eliminate
  the boilerplate associated with adding a new packet serializer.

```cpp
// The first argument is the name of the packet struct and
// the second is the name of the union variable used to
// access the data contained within the data struct.
SERIALIZER_WRITE(ExamplePacket, examplePacket)
SERIALIZER_READ(ExamplePacket, examplePacket)
SERIALIZER_BOTH(ExamplePacket, examplePacket)
```

- [ ] **Serializer Definition**: Based on the macro used, each packet is required to have
  a write and/or a read method defined for the serializer. This is done by creating a new file
  in [`src/serializers/`](../fom-network/src/serializers/) named `<PacketStruct>Serializer`.

```cpp
#include <fom-network/PacketSerializers.h>

void ExamplePacketSerializer::WriteData(
  RakNet::BitStream& bs, const FOMPacket::ExamplePacket& data) const {
  EncodeString(bs, data.exampleString);
  bs.WriteCompressed(data.exampleShort);
}

FOMPacket::ExamplePacket ExamplePacketSerializer::ReadData(RakNet::BitStream& bs) const {
  FOMPacket::ExamplePacket data{};
  DecodeString(bs, data.exampleString);
  bs.ReadCompressed(data.exampleShort);
  return data;
}
```

- [ ] **Enable Serializer**: Each serializer needs to be
  [included in a map](../fom-network/src/FOMDataSerializer.cpp) in order for the network
  library to make use of it when reading and writing packet data. It should only be
  added to the map that corresponds with the behavior that it implements.

```cpp
static std::unordered_map<uint32_t, IWriter*> writerMap = {
    ...
    {ID_EXAMPLE, &ExamplePacketSerializer::GetInstance()}};
static std::unordered_map<uint32_t, IReader*> readerMap = {
    ...
    {ID_EXAMPLE, &ExamplePacketSerializer::GetInstance()},
};
```

- [ ] **Interop Struct Validation**: Each packet struct needs to be added to
  [the validation map](../fom-network/src/NetworkAPI.cpp) so that it can be
  compared with the server's representation.

```cpp
// List all of the structs that we have defined in the library
// so that they can be compared to the consumer's structs.
std::unordered_map<uint8_t, uint32_t> libraryMap = {
  ...
  {ID_EXAMPLE, sizeof(FOMPacket::ExamplePacket)}
};
```

## [ServerShared](../server-shared/Core/Enums/PacketIdentifier.cs)

- [ ] **Packet Data**: Each native packet data struct requires a mirror copy [in the
  shared server class library](../server-shared/Core/Models/FOMData).

> [!CAUTION]
> Both the data types and field order must be identical!

```csharp
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models.FOMData
{
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

- [ ] **Data Union**: Each packet type must be added to
  [the managed struct data union](../server-shared/Core/Models/FOMData/FOMDataUnion.cs).

```csharp
// .NET does not really support unions but we can replicate the behavior by
// including a bunch of fields that have overlapping memory.
[StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct FOMDataUnion
    {
        ...
        [FieldOffset(0)] public ExamplePacket examplePacket;
    }
```

- [ ] **Packet Data Extraction**: As a convenience, packet data
  [can be unwrapped based on the identifier](../server-shared/Extensions/FOMPacketExtensions.cs).
  Each packet needs an entry so that it can be delegated to its handler without any complicated type casting.

```csharp
switch (packet.ID)
{
    ...
    case PacketIdentifier.ID_EXAMPLE when typeof(TPacket) == typeof(ExamplePacket):
        return (TPacket)(object)packet.Data.examplePacket;
}
```

- [ ] **Interop Struct Validation**: Before the server will be able to start, the new packet data struct must
  be added to [the managed validation map](../server-shared/Infrastructure/FOMNetwork/NetworkService.cs).

```csharp
{
    ...
    new PacketStructure { ID = PacketIdentifier.ID_EXAMPLE, Size = Marshal.SizeOf<ExamplePacket>() }
};
```

## Master/World Server

Although all of the packets are defined in the shared class library, each type will generally only be handled
bye one of the server types. Each packet handler is defined in their respective server projects.

- [ ] **Packet Handler**: Each handler should be created in their own
  `Application/PacketHandlers/<PacketType>Handler.cs` file.

```csharp
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.Shared.Application.PacketHandlers
{
    public class ExamplePacketHandler : PacketHandler<ReadPacketError>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_EXAMPLE;
        public override void Handle(NetworkAddress sender, in ExamplePacket data) { }
    }
}
```

- [ ] **Dependency Injection**: Packet handlers are registered with the dependency injection container in
  each project's `CompositionRoot.cs` file. The act of binding it to the container adds it to the packet
  handling pipeline and ensures that packets of the appropriate type are given to it.

```csharp
services.AddSingleton<IPacketHandler, ExamplePacketHandler>();
```
