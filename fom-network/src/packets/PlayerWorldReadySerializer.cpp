#include <fom-network/packets/PlayerWorldReady.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool PlayerWorldReadySerializer::Read(RakNet::BitStream& bs,
                                      Packet::PlayerWorldReady* data) const {
  if (!bs.ReadCompressed(data->playerID)) return false;
  if (!bs.ReadCompressed(data->status)) return false;
  return true;
}

void PlayerWorldReadySerializer::Write(
    RakNet::BitStream& bs, const Packet::PlayerWorldReady* data) const {
  bs.WriteCompressed(data->playerID);
  bs.WriteCompressed(data->status);
}

}  // namespace FOMNetwork
