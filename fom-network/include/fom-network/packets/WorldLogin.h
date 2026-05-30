#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldId.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldLogin {
  Enum::WorldId worldId;
  uint8_t nodeId;
  uint32_t playerId;
  uint32_t constant;  // 1293394
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldLogin);

}  // namespace Packet
}  // namespace FOMNetwork
