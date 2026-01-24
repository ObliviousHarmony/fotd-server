#pragma once

#include <fom-network/types/PlayerAttributes.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class PlayerAttributesSerializer
    : protected TypeSerializer<Type::PlayerAttributes> {
 public:
  void Write(RakNet::BitStream& bs, const Type::PlayerAttributes& data) const {
    for (int i = 0; i < Enum::NUM_PLAYER_ATTRIBUTES; ++i)
      bs.WriteCompressed(data.values[i]);
  }

  bool Read(RakNet::BitStream& bs, Type::PlayerAttributes& data) const {
    for (int i = 0; i < Enum::NUM_PLAYER_ATTRIBUTES; ++i) {
      if (!bs.ReadCompressed(data.values[i])) return false;
    }
    return true;
  }
};

}  // namespace FOMNetwork
