#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool LoginRequestSerializer::ReadData(RakNet::BitStream& bs,
                                      Packet::LoginRequest& data) const {
  if (!DecodeString(bs, data.username)) return false;
  if (!bs.ReadCompressed(data.clientVersion)) return false;
  return true;
}

}  // namespace FOMNetwork
