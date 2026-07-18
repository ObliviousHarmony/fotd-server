#pragma once

#include <fom-network/types/world/WorldService.h>

#include "../PositionRotationSerializer.h"
#include "../TypeSerializer.h"

namespace FOMNetwork {
namespace Type {

class WorldServiceSerializer : protected TypeSerializer<Type::WorldService> {
 public:
  void Write(RakNet::BitStream& bs, const Type::WorldService& data) const {
    PositionRotationSerializer positionRotationSerializer;

    uint32_t numPlacements = data.numPlacements;
    if (numPlacements > MAX_WORLD_SERVICE_PLACEMENTS)
      numPlacements = MAX_WORLD_SERVICE_PLACEMENTS;

    bs.WriteCompressed(data.id);
    bs.WriteCompressed(data.type);
    EncodeString(bs, data.modelPaths);
    EncodeString(bs, data.skinPaths);
    EncodeString(bs, data.renderStylePaths);
    bs.WriteCompressed(data.scale);
    bs.Write(data.moveToFloor == 1);
    bs.Write(data.isSolid == 1);
    bs.WriteCompressed(numPlacements);
    for (uint32_t i = 0; i < numPlacements; ++i) {
      bs.WriteCompressed(data.placementIds[i]);
      positionRotationSerializer.Write(bs, data.placements[i]);
    }
  }

  bool Read(RakNet::BitStream& bs, Type::WorldService& data) const {
    PositionRotationSerializer positionRotationSerializer;

    if (!bs.ReadCompressed(data.id)) return false;
    if (!bs.ReadCompressed(data.type)) return false;
    if (!DecodeString(bs, data.modelPaths)) return false;
    if (!DecodeString(bs, data.skinPaths)) return false;
    if (!DecodeString(bs, data.renderStylePaths)) return false;
    if (!bs.ReadCompressed(data.scale)) return false;

    bool moveToFloor, isSolid;
    if (!bs.Read(moveToFloor)) return false;
    if (!bs.Read(isSolid)) return false;
    data.moveToFloor = moveToFloor ? 1 : 0;
    data.isSolid = isSolid ? 1 : 0;

    if (!bs.ReadCompressed(data.numPlacements)) return false;
    if (data.numPlacements > MAX_WORLD_SERVICE_PLACEMENTS) return false;
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
