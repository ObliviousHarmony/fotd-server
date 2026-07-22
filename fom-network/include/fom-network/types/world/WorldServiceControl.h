#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/PositionRotation.h>

namespace FOMNetwork {
namespace Type {

constexpr int32_t MAX_WORLD_SERVICE_CONTROL_PLACEMENTS = 32;

#pragma pack(push, 1)
struct WorldServiceControl {
  uint32_t serviceId;
  uint8_t type;
  uint8_t state;
  uint8_t security;
  uint8_t target[65];
  uint32_t numPlacements;
  uint32_t placementIds[MAX_WORLD_SERVICE_CONTROL_PLACEMENTS];
  Type::PositionRotation placements[MAX_WORLD_SERVICE_CONTROL_PLACEMENTS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldServiceControl);

}  // namespace Type
}  // namespace FOMNetwork
