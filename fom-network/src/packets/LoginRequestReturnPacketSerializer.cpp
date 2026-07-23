#include <fom-network/packets/LoginRequestReturnPacket.h>

#include "PacketSerializers.h"

namespace FOMNetwork {

void LoginRequestReturnPacketSerializer::Write(
    RakNet::BitStream& bs, const LoginRequestReturnPacket* data) const {
  bs.WriteCompressed(data->status);
  EncodeString(bs, data->username);
}

}  // namespace FOMNetwork
