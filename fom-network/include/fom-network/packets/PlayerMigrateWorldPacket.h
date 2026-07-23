#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct PlayerMigrateWorldPacket {
  uint32_t playerId;
  uint32_t clientBinaryAddress;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerMigrateWorldPacket)

}  // namespace FOMNetwork
