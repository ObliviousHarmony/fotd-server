#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

Packet::LoginRequest LoginRequestSerializer::ReadData(
    RakNet::BitStream& bs) const {
  Packet::LoginRequest data{};
  DecodeString(bs, data.username);
  bs.ReadCompressed(data.clientVersion);
  return data;
}

}  // namespace FOMNetwork
