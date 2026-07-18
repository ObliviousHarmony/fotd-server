#pragma once

#include <fom-network/types/world/WorldObject.h>

#include "../PositionRotationSerializer.h"
#include "../TypeSerializer.h"

namespace FOMNetwork {
namespace Type {

class WorldObjectSerializer : protected TypeSerializer<Type::WorldObject> {
 public:
  void Write(RakNet::BitStream& bs, const Type::WorldObject& data) const {
    PositionRotationSerializer positionRotationSerializer;

    bs.WriteCompressed(data.id);
    bs.WriteCompressed(data.itemType);
    bs.WriteCompressed(data.state);
    bs.WriteCompressed(data.owningPlayerId);
    positionRotationSerializer.Write(bs, data.position);
  }

  bool Read(RakNet::BitStream& bs, Type::WorldObject& data) const {
    PositionRotationSerializer positionRotationSerializer;

    if (!bs.ReadCompressed(data.id)) return false;
    if (!bs.ReadCompressed(data.itemType)) return false;
    if (!bs.ReadCompressed(data.state)) return false;
    if (!bs.ReadCompressed(data.owningPlayerId)) return false;
    if (!positionRotationSerializer.Read(bs, data.position)) return false;

    return true;
  }
};

}  // namespace Type
}  // namespace FOMNetwork
