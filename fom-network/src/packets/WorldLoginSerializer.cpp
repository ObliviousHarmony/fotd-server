#include <fom-network/packets/WorldLogin.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool WorldLoginSerializer::Read(RakNet::BitStream& bs,
                                Packet::WorldLogin* data) const {
  if (!bs.ReadCompressed(data->worldId)) return false;
  if (!bs.ReadCompressed(data->nodeId)) return false;
  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.ReadCompressed(data->constant)) return false;

  return true;
}

}  // namespace FOMNetwork
