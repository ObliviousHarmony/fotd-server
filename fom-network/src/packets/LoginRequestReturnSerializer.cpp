#include <fom-network/packets/LoginRequestReturn.h>

#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

void LoginRequestReturnSerializer::Write(
    RakNet::BitStream& bs, const Packet::LoginRequestReturn* data) const {
  bs.WriteCompressed(data->status);
  EncodeString(bs, data->username);
}

}  // namespace Packet
}  // namespace FOMNetwork
