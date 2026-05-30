#include <fom-network/packets/PlayerMigrateWorld.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool PlayerMigrateWorldSerializer::Read(
    RakNet::BitStream& bs, Packet::PlayerMigrateWorld* data) const {
  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.Read(data->clientBinaryAddress)) return false;

  return true;
}

void PlayerMigrateWorldSerializer::Write(
    RakNet::BitStream& bs, const Packet::PlayerMigrateWorld* data) const {
  bs.WriteCompressed(data->playerId);
  bs.Write(data->clientBinaryAddress);
}

}  // namespace FOMNetwork
