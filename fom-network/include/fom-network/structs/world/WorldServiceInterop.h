#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldServiceType.h>
#include <fom-network/structs/PositionRotationInterop.h>

namespace FOMNetwork {

constexpr int32_t MAX_WORLD_SERVICE_PLACEMENTS = 32;

#pragma pack(push, 1)
struct WorldServiceInterop {
  uint32_t id;
  Enum::WorldServiceType type;
  uint8_t modelPaths[256];
  uint8_t skinPaths[256];
  uint8_t renderStylePaths[256];
  uint16_t scale;
  uint8_t moveToFloor;
  uint8_t isSolid;
  uint32_t numPlacements;
  uint32_t placementIds[MAX_WORLD_SERVICE_PLACEMENTS];
  PositionRotationInterop placements[MAX_WORLD_SERVICE_PLACEMENTS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldServiceInterop);

}  // namespace FOMNetwork
