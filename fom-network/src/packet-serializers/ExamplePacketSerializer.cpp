#include <raknet/BitStream.h>
#include <fom-network/FOMPacket.h>
#include <fom-network/PacketSerializers.h>

bool ExamplePacketSerializer::Serialize(RakNet::BitStream& bs, const ExamplePacket& p) const {
	bs.Write(p.exampleField1);
	return true;
}

ExamplePacket ExamplePacketSerializer::Deserialize(RakNet::BitStream& bs) const {
	ExamplePacket p;
	bs.Read(p.exampleField1);
	return p;
}
