#include <fom-network/packets/PlayerLeavingWorldPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool PlayerLeavingWorldPacketSerializer::Read(
    RakNet::BitStream& bs, PlayerLeavingWorldPacket* data) const {
  if (!bs.ReadCompressed(data->playerId)) return false;

  return true;
}

void PlayerLeavingWorldPacketSerializer::Write(
    RakNet::BitStream& bs, const PlayerLeavingWorldPacket* data) const {
  bs.WriteCompressed(data->playerId);
}

}  // namespace FOMNetwork
