#include <gtest/gtest.h>
#include <raknet/BitStream.h>
#include <fom-network/PacketSerializers.h>

// Equality operator for ExamplePacket to simplify assertions
bool operator==(const ExamplePacket& lhs, const ExamplePacket& rhs) {
    return lhs.exampleField1 == rhs.exampleField1;
}

TEST(ExamplePacketSerializerTest, RoundTrip) {
    ExamplePacketSerializer serializer;

    ExamplePacket input;
    input.exampleField1 = 42;

    // Serialize
    RakNet::BitStream bsOut;
    bool success = serializer.Serialize(bsOut, input);
    ASSERT_TRUE(success);

    // Reset for reading
    bsOut.ResetReadPointer();

    // Deserialize
    ExamplePacket output = serializer.Deserialize(bsOut);

    // Verify equality
    EXPECT_EQ(input, output);
}
