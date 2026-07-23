#include <fom-network/packets/WorldLoginPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool WorldLoginPacketSerializer::Read(RakNet::BitStream& bs,
                                      WorldLoginPacket* data) const {
  if (!bs.ReadCompressed(data->worldId)) return false;
  if (!bs.ReadCompressed(data->nodeId)) return false;
  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.ReadCompressed(data->constant)) return false;

  return true;
}

}  // namespace FOMNetwork
