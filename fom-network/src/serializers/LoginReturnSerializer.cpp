#include <fom-network/PacketSerializers.h>

void LoginReturnSerializer::WriteData(
    RakNet::BitStream& bs, const FOMPacket::LoginReturn& data) const {
  bs.WriteCompressed(data.status);
}
