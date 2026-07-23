#include <fom-network/packets/PlayerWorldReadyPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool PlayerWorldReadyPacketSerializer::Read(
    RakNet::BitStream& bs, PlayerWorldReadyPacket* data) const {
  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.ReadCompressed(data->status)) return false;

  return true;
}

void PlayerWorldReadyPacketSerializer::Write(
    RakNet::BitStream& bs, const PlayerWorldReadyPacket* data) const {
  bs.WriteCompressed(data->playerId);
  bs.WriteCompressed(data->status);
}

}  // namespace FOMNetwork
