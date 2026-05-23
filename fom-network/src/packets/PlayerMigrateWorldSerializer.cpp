#include <fom-network/packets/PlayerMigrateWorld.h>

#include "../types/NetworkAddressSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

bool PlayerMigrateWorldSerializer::Read(
    RakNet::BitStream& bs, Packet::PlayerMigrateWorld* data) const {
  NetworkAddressSerializer addressSerializer;

  if (!bs.ReadCompressed(data->playerID)) return false;
  if (!addressSerializer.Read(bs, data->clientAddress)) return false;

  return true;
}

void PlayerMigrateWorldSerializer::Write(
    RakNet::BitStream& bs, const Packet::PlayerMigrateWorld* data) const {
  NetworkAddressSerializer addressSerializer;

  bs.WriteCompressed(data->playerID);
  addressSerializer.Write(bs, data->clientAddress);
}

}  // namespace FOMNetwork
