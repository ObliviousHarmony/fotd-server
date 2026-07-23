#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldId.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct WorldLoginPacket {
  Enum::WorldId worldId;
  uint8_t nodeId;
  uint32_t playerId;
  uint32_t constant;  // 1293394
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldLoginPacket);

}  // namespace FOMNetwork
