#pragma once

#include <fom-network/PacketIdentifier.h>

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

namespace FOMPacket {
struct CheckNameReturn {
  uint32_t existingPlayerID;
};
ASSERT_BLITTABLE(CheckNameReturn);
}  // namespace FOMPacket

#pragma pack(pop)
