#include <fom-network/packets/CheckNamePacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool CheckNamePacketSerializer::Read(RakNet::BitStream& bs,
                                     CheckNamePacket* data) const {
  if (!DecodeString(bs, data->name)) return false;
  if (!bs.ReadCompressed(data->playerId)) return false;

  return true;
}

}  // namespace FOMNetwork
