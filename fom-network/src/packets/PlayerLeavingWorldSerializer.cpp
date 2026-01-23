#include <fom-network/packets/PlayerLeavingWorld.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool PlayerLeavingWorldSerializer::Read(
    RakNet::BitStream& bs, Packet::PlayerLeavingWorld* data) const {
  if (!bs.ReadCompressed(data->playerID)) return false;
  return true;
}

void PlayerLeavingWorldSerializer::Write(
    RakNet::BitStream& bs, const Packet::PlayerLeavingWorld* data) const {
  bs.WriteCompressed(data->playerID);
}

}  // namespace FOMNetwork
