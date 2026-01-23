#include <fom-network/packets/PlayerWorldReady.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool PlayerWorldReadySerializer::Read(RakNet::BitStream& bs,
                                      Packet::PlayerWorldReady* data) const {
  if (!bs.ReadCompressed(data->playerID)) return false;
  return true;
}

void PlayerWorldReadySerializer::Write(
    RakNet::BitStream& bs, const Packet::PlayerWorldReady* data) const {
  bs.WriteCompressed(data->playerID);
}

}  // namespace FOMNetwork
