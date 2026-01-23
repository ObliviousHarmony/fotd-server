#include <fom-network/packets/WorldLogin.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool WorldLoginSerializer::Read(RakNet::BitStream& bs,
                                Packet::WorldLogin* data) const {
  if (!bs.ReadCompressed(data->worldID)) return false;
  if (!bs.ReadCompressed(data->nodeID)) return false;
  if (!bs.ReadCompressed(data->playerID)) return false;
  if (!bs.ReadCompressed(data->constant)) return false;
  return true;
}

}  // namespace FOMNetwork
