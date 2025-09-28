#pragma once

#include <fom-network/PacketIdentifier.h>

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

namespace FOMPacket {
struct CreateCharacter {
  uint8_t name[20];
  uint8_t biography[511];
};
ASSERT_BLITTABLE(CreateCharacter);
}  // namespace FOMPacket

#pragma pack(pop)
