#include <fom-network/packets/LoginRequest.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

bool LoginRequestSerializer::Read(RakNet::BitStream& bs,
                                  Packet::LoginRequest* data) const {
  if (!DecodeString(bs, data->username)) return false;
  if (!bs.ReadCompressed(data->clientVersion)) return false;

  return true;
}

}  // namespace FOMNetwork
