#pragma once

#include <fom-network/constants/FactionConstants.h>
#include <fom-network/types/faction/FactionEmblem.h>

#include "../TypeSerializer.h"
#include "FactionEmblemLayerSerializer.h"

namespace FOMNetwork {
namespace Type {

class FactionEmblemSerializer : protected TypeSerializer<Type::FactionEmblem> {
 public:
  void Write(RakNet::BitStream& bs, const Type::FactionEmblem& data) const {
    FactionEmblemLayerSerializer layerSerializer;

    bs.WriteCompressed(data.staticEmblemId);
    for (int i = 0; i < Constants::NUM_FACTION_EMBLEM_LAYERS; ++i)
      layerSerializer.Write(bs, data.layers[i]);
  }

  bool Read(RakNet::BitStream& bs, Type::FactionEmblem& data) const {
    FactionEmblemLayerSerializer layerSerializer;

    if (!bs.ReadCompressed(data.staticEmblemId)) return false;
    for (int i = 0; i < Constants::NUM_FACTION_EMBLEM_LAYERS; ++i) {
      if (!layerSerializer.Read(bs, data.layers[i])) return false;
    }

    return true;
  }
};

}  // namespace Type
}  // namespace FOMNetwork
