#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct CheckNamePacket {
  uint8_t name[BufferSizes::PLAYER_NAME];
  uint32_t playerId;
};
#pragma pack(pop)

ASSERT_BLITTABLE(CheckNamePacket);

}  // namespace FOMNetwork
