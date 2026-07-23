#include <fom-network/packets/RegisterClientPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool RegisterClientPacketSerializer::Read(RakNet::BitStream& bs,
                                          RegisterClientPacket* data) const {
  if (!bs.ReadCompressed(data->worldId)) return false;
  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.ReadCompressed(data->worldCrc)) return false;

  return true;
}

}  // namespace FOMNetwork
