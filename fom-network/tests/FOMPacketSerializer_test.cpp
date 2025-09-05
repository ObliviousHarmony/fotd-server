#include <gtest/gtest.h>
#include <raknet/BitStream.h>
#include <fom-network/FOMPacketSerializer.h>

// Demonstrate some basic assertions.
TEST(FOMPacketSerializer, DeserializeMissingPacketID) {
	RakNet::BitStream bs;
	
	FOMPacket packet = FOMPacketSerializer::Deserialize(bs);

	ASSERT_EQ(packet.ID, ID_FOM_PACKET_ERROR);
	ASSERT_EQ(packet.data.error.errorPacketID, ID_FOM_PACKET_ERROR);
	ASSERT_EQ(packet.data.error.errorCode, FOMPacketErrorCode::ERROR_MISSING_PACKET_ID);
}

TEST(FOMPacketSerializer, DeserializeUnhandledPacketID) {
	// Use a RakNet internal ID which will have no deserializer.
	RakNet::BitStream bs;
	bs.Write((uint8_t)1);

	FOMPacket packet = FOMPacketSerializer::Deserialize(bs);

	ASSERT_EQ(packet.ID, ID_FOM_PACKET_ERROR);
	ASSERT_EQ(packet.data.error.errorPacketID, (PacketIdentifier)1);
	ASSERT_EQ(packet.data.error.errorCode, FOMPacketErrorCode::ERROR_UNHANDLED_PACKET_ID);
}
