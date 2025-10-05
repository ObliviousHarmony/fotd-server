#include <fom-network/packets/PacketSerializers.h>

namespace FOMNetwork {

bool LoginRequestSerializer::ReadData(RakNet::BitStream& bs,
                                      Packet::LoginRequest& data) const {
  DecodeString(bs, data.username);
  bs.ReadCompressed(data.clientVersion);

  return true;
}

}  // namespace FOMNetwork
