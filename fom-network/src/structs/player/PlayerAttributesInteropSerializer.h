#pragma once

#include <fom-network/structs/player/PlayerAttributesInterop.h>

#include "../InteropTypeSerializer.h"

namespace FOMNetwork {

class PlayerAttributesInteropSerializer
    : protected InteropTypeSerializer<PlayerAttributesInterop> {
 public:
  void Write(RakNet::BitStream& bs, const PlayerAttributesInterop& data) const {
    for (int i = 0; i < Enum::NUM_ATTRIBUTE_TYPES; ++i)
      bs.WriteCompressed(data.values[i]);
  }

  bool Read(RakNet::BitStream& bs, PlayerAttributesInterop& data) const {
    for (int i = 0; i < Enum::NUM_ATTRIBUTE_TYPES; ++i) {
      if (!bs.ReadCompressed(data.values[i])) return false;
    }
    return true;
  }
};

}  // namespace FOMNetwork
