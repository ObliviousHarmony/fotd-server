#include <fom-network/PacketSerializers.h>

FOMPacket::LoginRequest LoginRequestSerializer::ReadData(RakNet::BitStream& bs) const {
	FOMPacket::LoginRequest data{};
	DecodeString(bs, data.username);
	bs.ReadCompressed(data.clientVersion);
	return data;
}
