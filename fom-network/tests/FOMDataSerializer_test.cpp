#include <fom-network/FOMDataSerializer.h>

#pragma warning(push)
#pragma warning(disable : 26495)

#include <gtest/gtest.h>
#include <raknet/BitStream.h>

#pragma warning(pop)

using namespace FOMNetwork;

TEST(FOMDataSerializer, ReadUnhandledPacketID) {
  uint8_t* buffer = new uint8_t[1];
  RakNet::BitStream bs;

  ASSERT_FALSE(FOMDataSerializer::Read(bs, (PacketIdentifier)ID_INTERNAL_PING, buffer));

  delete buffer;
}

TEST(FOMDataSerializer, ForwardCertainRakNetID) {
  uint8_t* buffer = new uint8_t[1];
  RakNet::BitStream bs;

  ASSERT_TRUE(FOMDataSerializer::Read(
      bs, (PacketIdentifier)ID_NEW_INCOMING_CONNECTION, buffer));

  delete buffer;
}
