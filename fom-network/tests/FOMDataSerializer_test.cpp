#include "../src/FOMDataSerializer.h"

#pragma warning(push)
#pragma warning(disable : 26495)

#include <gtest/gtest.h>
#pragma warning(pop)

using namespace FOMNetwork;

TEST(FOMDataSerializer, ReadUnhandledPacketId) {
  uint8_t* buffer = new uint8_t[1];
  RakNet::BitStream bs;

  ASSERT_FALSE(FOMDataSerializer::Read(
      bs, (Enum::PacketIdentifier)ID_INTERNAL_PING, buffer));

  delete buffer;
}

TEST(FOMDataSerializer, ForwardCertainRakNetId) {
  uint8_t* buffer = new uint8_t[1];
  RakNet::BitStream bs;

  ASSERT_TRUE(FOMDataSerializer::Read(
      bs, (Enum::PacketIdentifier)ID_NEW_INCOMING_CONNECTION, buffer));

  delete buffer;
}
