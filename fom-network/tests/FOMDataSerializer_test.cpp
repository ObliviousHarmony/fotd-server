#include <gtest/gtest.h>
#include <raknet/BitStream.h>
#include <fom-network/FOMDataSerializer.h>

TEST(FOMDataSerializer, DeserializeUnhandledPacketID) {
	RakNet::BitStream bs;

	try {
		FOMDataSerializer::Deserialize(bs, (PacketIdentifier)ID_INTERNAL_PING);
		FAIL() << "Expected DeserializationError";
	} catch (const FOMDataSerializer::DeserializationError& e) {
		ASSERT_EQ(e.error.offendingID, ID_INTERNAL_PING);
		ASSERT_EQ(e.error.errorCode, FOMPacketErrorCode::ERROR_UNHANDLED_PACKET_ID);
	} catch (...) {
		FAIL() << "Expected DeserializationError";
	}
}

TEST(FOMDataSerializer, ForwardCertainRakNetID) {
	RakNet::BitStream bs;

	// Will not throw since this is a handled RakNet ID.
	FOMDataSerializer::Deserialize(bs, (PacketIdentifier)ID_NEW_INCOMING_CONNECTION);
}
