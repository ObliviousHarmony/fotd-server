#pragma once

#include <fom-network/structs/world/WorldObjectInterop.h>

#include "../InteropTypeSerializer.h"
#include "../PositionRotationInteropSerializer.h"

namespace FOMNetwork {

class WorldObjectSerializer
    : protected InteropTypeSerializer<WorldObjectInterop> {
 public:
  void Write(RakNet::BitStream& bs, const WorldObjectInterop& data) const {
    PositionRotationInteropSerializer positionRotationSerializer;

    bs.WriteCompressed(data.id);
    bs.WriteCompressed(data.itemType);
    bs.WriteCompressed(data.state);
    bs.WriteCompressed(data.owningPlayerId);
    positionRotationSerializer.Write(bs, data.position);
  }

  bool Read(RakNet::BitStream& bs, WorldObjectInterop& data) const {
    PositionRotationInteropSerializer positionRotationSerializer;

    if (!bs.ReadCompressed(data.id)) return false;
    if (!bs.ReadCompressed(data.itemType)) return false;
    if (!bs.ReadCompressed(data.state)) return false;
    if (!bs.ReadCompressed(data.owningPlayerId)) return false;
    if (!positionRotationSerializer.Read(bs, data.position)) return false;

    return true;
  }
};

}  // namespace FOMNetwork
