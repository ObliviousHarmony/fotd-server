#include <fom-network/packets/CheckName.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool CheckNameSerializer::Read(RakNet::BitStream& bs,
                               Packet::CheckName* data) const {
  if (!DecodeString(bs, data->name)) return false;
  if (!bs.ReadCompressed(data->playerId)) return false;

  return true;
}

}  // namespace FOMNetwork
