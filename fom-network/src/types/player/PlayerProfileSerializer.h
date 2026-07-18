#pragma once

#include <fom-network/types/player/PlayerProfile.h>

#include "../TypeSerializer.h"

namespace FOMNetwork {
namespace Type {

class PlayerProfileSerializer : protected TypeSerializer<Type::PlayerProfile> {
 public:
  void Write(RakNet::BitStream& bs, const Type::PlayerProfile& data) const {
    bs.WriteCompressed(data.playerId);
    bs.Write(data.unknown1 == 1);
    EncodeString(bs, data.playerName);
    EncodeString(bs, data.factionName);
    EncodeString(bs, data.biography);
    EncodeString(bs, data.rankName);
  }

  bool Read(RakNet::BitStream& bs, Type::PlayerProfile& data) const {
    if (!bs.ReadCompressed(data.playerId)) return false;
    bool unknown1;
    if (!bs.Read(unknown1)) return false;
    data.unknown1 = unknown1 ? 1 : 0;
    if (!DecodeString(bs, data.playerName)) return false;
    if (!DecodeString(bs, data.factionName)) return false;
    if (!DecodeString(bs, data.biography)) return false;
    if (!DecodeString(bs, data.rankName)) return false;
    return true;
  }
};

}  // namespace Type
}  // namespace FOMNetwork
