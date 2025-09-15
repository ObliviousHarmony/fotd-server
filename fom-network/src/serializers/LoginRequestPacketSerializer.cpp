#include <fom-network/PacketSerializers.h>

LoginRequestPacket LoginRequestPacketSerializer::ReadData(RakNet::BitStream& bs) const {
	LoginRequestPacket data{};
	DecodeString(bs, data.username);
	bs.ReadCompressed(data.clientVersion);
	return data;
}
