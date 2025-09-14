#include <cstring>
#include <gtest/gtest.h>
#include <raknet/BitStream.h>
#include <fom-network/PacketSerializers.h>

static bool operator==(const FOMPacketError& lhs, const FOMPacketError& rhs) {
	return std::memcmp(&lhs, &rhs, sizeof(FOMPacketError)) == 0;
}

TEST(FOMPacketErrorSerializerTest, RoundTrip) {
	FOMPacketErrorSerializer serializer;

	FOMPacketError input{};
	input.offendingID = ID_FOM_PACKET_START;
	input.errorCode = FOMPacketErrorCode::ERROR_UNHANDLED_PACKET_ID;

	// Serialize
	RakNet::BitStream bsOut;
	bool success = serializer.Serialize(bsOut, input);
	ASSERT_TRUE(success);

	// Reset for reading
	bsOut.ResetReadPointer();

	// Deserialize
	FOMPacketError output = serializer.Deserialize(bsOut);

	// Verify equality
	EXPECT_EQ(input, output);
}
