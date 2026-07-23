#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/WorldUpdateInterop.h>

namespace FOMNetwork {

constexpr int MAX_WORLD_UPDATES = 100;

#pragma pack(push, 1)
struct WorldUpdatePacket {
  uint32_t playerId;
  uint32_t unknown1;
  uint8_t updateCount;
  WorldUpdateInterop updates[MAX_WORLD_UPDATES];
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldUpdatePacket);

}  // namespace FOMNetwork
