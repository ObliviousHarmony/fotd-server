#include <fom-network/packets/LoginRequestPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool LoginRequestPacketSerializer::Read(RakNet::BitStream& bs,
                                        LoginRequestPacket* data) const {
  if (!DecodeString(bs, data->username)) return false;
  if (!bs.ReadCompressed(data->clientVersion)) return false;

  return true;
}

}  // namespace FOMNetwork
