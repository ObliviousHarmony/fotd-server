#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/PositionRotationInterop.h>

namespace FOMNetwork {

constexpr int32_t MAX_WORLD_SERVICE_CONTROL_PLACEMENTS = 32;

#pragma pack(push, 1)
struct WorldServiceControlInterop {
  uint32_t serviceId;
  uint8_t type;
  uint8_t state;
  uint8_t security;
  uint8_t target[65];
  uint32_t numPlacements;
  uint32_t placementIds[MAX_WORLD_SERVICE_CONTROL_PLACEMENTS];
  PositionRotationInterop placements[MAX_WORLD_SERVICE_CONTROL_PLACEMENTS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldServiceControlInterop);

}  // namespace FOMNetwork
