#include <fom-network/packets/LoginRequestReturn.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

void LoginRequestReturnSerializer::Write(
    RakNet::BitStream& bs, const Packet::LoginRequestReturn* data) const {
  bs.WriteCompressed(data->status);
  EncodeString(bs, data->username);
}

}  // namespace FOMNetwork
