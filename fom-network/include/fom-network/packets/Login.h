#pragma once

#include <fom-network/PacketIdentifier.h>

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

namespace FOMPacket {
struct Login {
  uint8_t username[19];
  uint8_t passwordHash[32];
  uint32_t clientCRC;
  uint32_t cshellCRC;
  uint32_t objectCRC;
  uint8_t macAddress[18];
};
ASSERT_BLITTABLE(Login);
}  // namespace FOMPacket

#pragma pack(pop)
