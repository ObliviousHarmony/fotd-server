#pragma once

#include <fom-network/structs/world/WorldServiceControlInterop.h>

#include "../InteropTypeSerializer.h"
#include "../PositionRotationInteropSerializer.h"

namespace FOMNetwork {

class WorldServiceControlSerializer
    : protected InteropTypeSerializer<WorldServiceControlInterop> {
 public:
  void Write(RakNet::BitStream& bs,
             const WorldServiceControlInterop& data) const {
    PositionRotationInteropSerializer positionRotationSerializer;

    uint32_t numPlacements = data.numPlacements;
    if (numPlacements > MAX_WORLD_SERVICE_CONTROL_PLACEMENTS)
      numPlacements = MAX_WORLD_SERVICE_CONTROL_PLACEMENTS;

    bs.WriteCompressed(data.serviceId);
    bs.WriteCompressed(data.type);
    bs.WriteCompressed(data.state);
    bs.WriteCompressed(data.security);
    EncodeString(bs, data.target);
    bs.WriteCompressed(numPlacements);
    for (uint32_t i = 0; i < numPlacements; ++i) {
      bs.WriteCompressed(data.placementIds[i]);
      positionRotationSerializer.Write(bs, data.placements[i]);
    }
  }

  bool Read(RakNet::BitStream& bs, WorldServiceControlInterop& data) const {
    PositionRotationInteropSerializer positionRotationSerializer;

    if (!bs.ReadCompressed(data.serviceId)) return false;
    if (!bs.ReadCompressed(data.type)) return false;
    if (!bs.ReadCompressed(data.state)) return false;
    if (!bs.ReadCompressed(data.security)) return false;
    if (!DecodeString(bs, data.target)) return false;

    if (!bs.ReadCompressed(data.numPlacements)) return false;
    if (data.numPlacements > MAX_WORLD_SERVICE_CONTROL_PLACEMENTS) return false;
    for (uint32_t i = 0; i < data.numPlacements; ++i) {
      if (!bs.ReadCompressed(data.placementIds[i])) return false;
      if (!positionRotationSerializer.Read(bs, data.placements[i]))
        return false;
    }

    return true;
  }
};

}  // namespace FOMNetwork
