#pragma once

#include <fom-network/constants/FactionConstants.h>
#include <fom-network/structs/faction/FactionEmblemInterop.h>

#include "../InteropTypeSerializer.h"
#include "FactionEmblemLayerInteropSerializer.h"

namespace FOMNetwork {

class FactionEmblemInteropSerializer
    : protected InteropTypeSerializer<FactionEmblemInterop> {
 public:
  void Write(RakNet::BitStream& bs, const FactionEmblemInterop& data) const {
    FactionEmblemLayerInteropSerializer layerSerializer;

    bs.WriteCompressed(data.staticEmblemId);
    for (int i = 0; i < Constants::NUM_FACTION_EMBLEM_LAYERS; ++i)
      layerSerializer.Write(bs, data.layers[i]);
  }

  bool Read(RakNet::BitStream& bs, FactionEmblemInterop& data) const {
    FactionEmblemLayerInteropSerializer layerSerializer;

    if (!bs.ReadCompressed(data.staticEmblemId)) return false;
    for (int i = 0; i < Constants::NUM_FACTION_EMBLEM_LAYERS; ++i) {
      if (!layerSerializer.Read(bs, data.layers[i])) return false;
    }

    return true;
  }
};

}  // namespace FOMNetwork
