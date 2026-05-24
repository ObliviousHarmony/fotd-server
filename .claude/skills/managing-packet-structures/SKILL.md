---
name: managing-packet-structures
description: Use this when adding, removing, or editing packet structures. Do not use when building packet handlers.
---

# Managing Packet Structures

> **Feature-complete constraint**: This is a server emulator for an immutable, discontinued client. Packet structures and their enums mirror what the original game client expects on the wire — they are **feature-complete and frozen**. Never add status codes, fields, or new packet types to packets the client sends or receives. The structs that exist are the complete set. If a flow seems to need a new code/field, reuse an existing value, handle it server-side without a response, or defer to out-of-band mechanisms (e.g. forcible disconnect) — do not extend the packet.

Packets are blittable structs passed between managed and native code. These packets _must_ be blittable in order for them to function properly and follow zero-copy semantics. `struct` layout, packing, and data types must be identical.

- `bool` is not blittable and is passed around as `uint8_t`

## Managed Code

The ServerShared project contains [the managed packet structures](/server-shared/Core/Packets/Data).

- [PacketIdentifier Enum](/server-shared/Core/Enums/PacketIdentifier.cs) matches native code.
- Required `struct` Attributes:
  - `[PacketID(PacketIdentifier.{VALUE})]` hooks into validation and testing.
  - `[StructLayout(LayoutKind.Sequential, Pack = 1)]` for blittability.
- Strings are fixed-length `byte` arrays that map to C-style strings in native code.
- Variable-length arrays of data have a fixed-length buffer and a size field.
- `InlineArray` attribute for arrays of other blittable structs.
- `uint8_t` bools are parsed using a property getter.
- C-style strings are parsed into `string` types using a property getter.
- [Models contain shared data](/server-shared/Core/Packets/Models/).

## Native Code

The FOMNetwork project contains [the native packet structures](/fom-network/include/fom-network/packets/data) as well as [the BitStream serializers](/fom-network/src/packet-serializers/data). Keep [the list of sizes and serializers maintained](/fom-network/src/FOMDataSerializer.cpp).

### Structures

- [PacketIdentifier Enum](/fom-network/include/fom-network/packets/PacketIdentifier.h) matches managed code.
- `#pragma pack(push, 1)` and `#pragma pack(pop)` wrapping `struct` definition for data and models.
- `ASSERT_BLITTABLE()` macro does basic compile-time checks.
- Only use `<cstdint>` `_t` types for packet data.
- [Models contain shared data](/fom-network/include/fom-network/packets/models/).

### Serializers

- Use [serializer macros](/fom-network/include/fom-network/packets/PacketSerializers.h) to avoid creating headers for each serializer.
- Packets Read/Write using `RakNet::BitStream` with identical read/write paths.
- `EncodeString/DecodeString()` in packet serializers for compressed strings.
- `WriteRawString/ReadRawString` for raw C-style strings.
- `WriteBits/ReadBits` for specific lengths.
- `bs.Read()` to read bools and `bs.Write(val == 1)` for writing `uint8_t` bools.
- [Models have dedicated serializers](/fom-network/src/packet-serializers/models).

## Examples

- Login Return Packet
  - [Managed Struct](/server-shared/Core/Packets/Data/LoginReturn.cs)
  - [Native Struct](/fom-network/include/fom-network/packets/data/LoginReturn.h)
  - [Native Serializer](/fom-network/src/packet-serializers/data/LoginReturnSerializer.cpp)
