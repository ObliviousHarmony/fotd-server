#include <fom-network/packets/RegisterClient.h>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

bool RegisterClientSerializer::Read(RakNet::BitStream& bs,
                                    Packet::RegisterClient* data) const {
  if (!bs.ReadCompressed(data->worldId)) return false;
  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.ReadCompressed(data->worldCrc)) return false;

  return true;
}

}  // namespace Packet
}  // namespace FOMNetwork
