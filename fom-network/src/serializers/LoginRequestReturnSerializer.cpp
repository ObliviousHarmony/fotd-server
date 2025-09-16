#include <fom-network/PacketSerializers.h>

void LoginRequestReturnSerializer::WriteData(RakNet::BitStream& bs, const LoginRequestReturn& data) const {
	bs.WriteCompressed(data.status);
	EncodeString(bs, data.username);
}
