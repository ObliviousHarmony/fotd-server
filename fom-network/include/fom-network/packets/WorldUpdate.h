#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/WorldUpdate.h>

namespace FOMNetwork {
namespace Packet {

constexpr int MAX_WORLD_UPDATES = 100;

#pragma pack(push, 1)
struct WorldUpdate {
  uint32_t playerId;
  uint32_t unknown1;
  uint16_t updateCount;
  Type::WorldUpdate updates[MAX_WORLD_UPDATES];
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldUpdate);

}  // namespace Packet
}  // namespace FOMNetwork
