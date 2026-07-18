#pragma once

#include <fom-network/types/world/WorldServiceControl.h>

#include "../PositionRotationSerializer.h"
#include "../TypeSerializer.h"

namespace FOMNetwork {
namespace Type {

class WorldServiceControlSerializer
    : protected TypeSerializer<Type::WorldServiceControl> {
 public:
  void Write(RakNet::BitStream& bs,
             const Type::WorldServiceControl& data) const {
    PositionRotationSerializer positionRotationSerializer;

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

  bool Read(RakNet::BitStream& bs, Type::WorldServiceControl& data) const {
    PositionRotationSerializer positionRotationSerializer;

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

}  // namespace Type
}  // namespace FOMNetwork
