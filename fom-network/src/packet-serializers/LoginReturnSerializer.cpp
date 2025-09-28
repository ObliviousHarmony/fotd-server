#include <fom-network/PacketSerializers.h>

namespace FOMNetwork {

void LoginReturnSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::LoginReturn& data) const {
  bs.WriteCompressed(data.status);
}

}  // namespace FOMNetwork
