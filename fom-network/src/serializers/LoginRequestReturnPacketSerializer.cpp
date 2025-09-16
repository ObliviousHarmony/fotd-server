#include <fom-network/PacketSerializers.h>

void LoginRequestReturnPacketSerializer::WriteData(RakNet::BitStream& bs, const LoginRequestReturnPacket& data) const {
	bs.WriteCompressed(data.status);
	EncodeString(bs, data.username);
}
