#include <fom-network/FOMDataSerializer.h>
#include <gtest/gtest.h>
#include <raknet/BitStream.h>

using namespace FOMNetwork;

TEST(FOMDataSerializer, ReadUnhandledPacketID) {
  RakNet::BitStream bs;

  try {
    FOMDataSerializer::Read(bs, (PacketIdentifier)ID_INTERNAL_PING);
    FAIL() << "Expected ReadError";
  } catch (const FOMDataSerializer::ReadError& e) {
    ASSERT_EQ(e.readError.offendingID, ID_INTERNAL_PING);
    ASSERT_EQ(e.readError.errorCode,
              FOMNetwork::Packet::ReadPacketErrorCode::ERROR_UNHANDLED_PACKET_ID);
  } catch (...) {
    FAIL() << "Expected ReadError";
  }
}

TEST(FOMDataSerializer, ForwardCertainRakNetID) {
  RakNet::BitStream bs;

  // Will not throw since this is a handled RakNet ID.
  FOMDataSerializer::Read(bs, (PacketIdentifier)ID_NEW_INCOMING_CONNECTION);
}
