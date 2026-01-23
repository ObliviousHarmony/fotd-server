#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldID.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct WorldLogin {
  Enum::WorldID worldID;
  uint8_t nodeID;
  uint32_t playerID;
  uint32_t constant;  // 1293394
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldLogin);

}  // namespace Packet
}  // namespace FOMNetwork
