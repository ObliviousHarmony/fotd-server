#include <fom-network/packets/RegisterClient.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool RegisterClientSerializer::Read(RakNet::BitStream& bs,
                                    Packet::RegisterClient* data) const {
  if (!bs.ReadCompressed(data->worldID)) return false;
  if (!bs.ReadCompressed(data->playerID)) return false;
  if (!bs.ReadCompressed(data->worldCRC)) return false;

  return true;
}

}  // namespace FOMNetwork
