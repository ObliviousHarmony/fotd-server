#include <fom-network/packets/PlayerMigrateWorldPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool PlayerMigrateWorldPacketSerializer::Read(
    RakNet::BitStream& bs, PlayerMigrateWorldPacket* data) const {
  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.Read(data->clientBinaryAddress)) return false;

  return true;
}

void PlayerMigrateWorldPacketSerializer::Write(
    RakNet::BitStream& bs, const PlayerMigrateWorldPacket* data) const {
  bs.WriteCompressed(data->playerId);
  bs.Write(data->clientBinaryAddress);
}

}  // namespace FOMNetwork
