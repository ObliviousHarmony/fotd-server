#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

void LoginRequestReturnSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::LoginRequestReturn& data) const {
  bs.WriteCompressed(data.status);
  EncodeString(bs, data.username);
}

}  // namespace FOMNetwork
