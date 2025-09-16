#include <fom-network/PacketSerializers.h>

LoginRequest LoginRequestSerializer::ReadData(RakNet::BitStream& bs) const {
	LoginRequest data{};
	DecodeString(bs, data.username);
	bs.ReadCompressed(data.clientVersion);
	return data;
}
