#pragma once

#include <fom-network/structs/player/PlayerProfileInterop.h>

#include "../InteropTypeSerializer.h"

namespace FOMNetwork {

class PlayerProfileInteropSerializer
    : protected InteropTypeSerializer<PlayerProfileInterop> {
 public:
  void Write(RakNet::BitStream& bs, const PlayerProfileInterop& data) const {
    bs.WriteCompressed(data.playerId);
    bs.Write(data.unknown1 == 1);
    EncodeString(bs, data.playerName);
    EncodeString(bs, data.factionName);
    EncodeString(bs, data.biography);
    EncodeString(bs, data.rankName);
  }

  bool Read(RakNet::BitStream& bs, PlayerProfileInterop& data) const {
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

}  // namespace FOMNetwork
