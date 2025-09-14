#include <raknet/BitStream.h>
#include <fom-network/FOMPacket.h>
#include <fom-network/PacketSerializers.h>

bool FOMPacketErrorSerializer::Serialize(RakNet::BitStream& bs, const FOMPacketError& data) const {
	bs.Write(data.offendingID);
	bs.Write(data.errorCode);
	return true;
}

FOMPacketError FOMPacketErrorSerializer::Deserialize(RakNet::BitStream& bs) const {
	FOMPacketError error{};
	bs.Read(error.offendingID);
	bs.Read(error.errorCode);
	return error;
}
