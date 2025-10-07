#include <fom-network/FOMDataSerializer.h>

#pragma warning(push)
#pragma warning(disable : 26495)

#include <gtest/gtest.h>
#include <raknet/BitStream.h>

#pragma warning(pop)

using namespace FOMNetwork;

TEST(FOMDataSerializer, ReadUnhandledPacketID) {
  RakNet::BitStream bs;

  // Create a buffer big enough to hold the packet.
  uint8_t* buffer = new uint8_t[1024];
  FOMDataSerializer::Read(bs, (PacketIdentifier)ID_INTERNAL_PING, buffer);

  FOMNetwork::Packet::ReadPacketError* e =
      reinterpret_cast<FOMNetwork::Packet::ReadPacketError*>(buffer);
  ASSERT_EQ(e->offendingID, ID_INTERNAL_PING);
  ASSERT_EQ(e->errorCode,
            FOMNetwork::Packet::ReadPacketErrorCode::ERROR_UNHANDLED_PACKET_ID);
}

TEST(FOMDataSerializer, ForwardCertainRakNetID) {
  RakNet::BitStream bs;

  uint8_t* buffer = new uint8_t[1024];
  FOMDataSerializer::Read(bs, (PacketIdentifier)ID_NEW_INCOMING_CONNECTION,
                          buffer);
  FOMNetwork::Packet::NewIncomingConnection* e =
      reinterpret_cast<FOMNetwork::Packet::NewIncomingConnection*>(buffer);
}
