#include <fom-network/FOMDataSerializer.h>

#include <unordered_map>

namespace FOMNetwork {

/**
 * We need to initialize the map with all of the serializers we want to be able
 * to use.
 */
static std::unordered_map<uint32_t, IWriter*> writerMap = {
    {ID_LOGIN_REQUEST_RETURN, &LoginRequestReturnSerializer::GetInstance()},
    {ID_LOGIN_RETURN, &LoginReturnSerializer::GetInstance()},
    {ID_CHECK_NAME_RETURN, &CheckNameReturnSerializer::GetInstance()},
    {ID_REGISTER_WORLD, &RegisterWorldSerializer::GetInstance()},
    {ID_WORLD_OVERVIEW_RETURN, &WorldOverviewReturnSerializer::GetInstance()},
    {ID_WORLD_LOGIN_RETURN, &WorldLoginReturnSerializer::GetInstance()},
};

static std::unordered_map<uint32_t, IReader*> readerMap = {
    // Some RakNet packets will be forwarded to the consumer.
    {ID_ALREADY_CONNECTED, &EmptyPacketSerializer::GetInstance()},
    {ID_CONNECTION_ATTEMPT_FAILED, &EmptyPacketSerializer::GetInstance()},
    {ID_CONNECTION_BANNED, &EmptyPacketSerializer::GetInstance()},
    {ID_CONNECTION_LOST, &EmptyPacketSerializer::GetInstance()},
    {ID_CONNECTION_REQUEST_ACCEPTED, &EmptyPacketSerializer::GetInstance()},
    {ID_DISCONNECTION_NOTIFICATION, &EmptyPacketSerializer::GetInstance()},
    {ID_INVALID_PASSWORD, &EmptyPacketSerializer::GetInstance()},
    {ID_MODIFIED_PACKET, &EmptyPacketSerializer::GetInstance()},
    {ID_NEW_INCOMING_CONNECTION, &EmptyPacketSerializer::GetInstance()},
    {ID_NO_FREE_INCOMING_CONNECTIONS, &EmptyPacketSerializer::GetInstance()},
    {ID_RSA_PUBLIC_KEY_MISMATCH, &EmptyPacketSerializer::GetInstance()},

    // Game Packets
    {ID_LOGIN_REQUEST, &LoginRequestSerializer::GetInstance()},
    {ID_LOGIN, &LoginSerializer::GetInstance()},
    {ID_CHECK_NAME, &CheckNameSerializer::GetInstance()},
    {ID_CREATE_CHARACTER, &CreateCharacterSerializer::GetInstance()},
    {ID_REGISTER_WORLD, &RegisterWorldSerializer::GetInstance()},
    {ID_WORLD_OVERVIEW, &WorldOverviewSerializer::GetInstance()},
    {ID_WORLD_LOGIN, &WorldLoginSerializer::GetInstance()},
};

bool FOMDataSerializer::Write(RakNet::BitStream& bs, const PacketIdentifier id,
                              const FOMDataUnion& data) {
  const auto* writer = GetWriter(id);
  if (!writer) {
    return false;
  }

  // Make sure to catch any serialization error so that the
  // library does not crash the consuming application.
  try {
    writer->Write(bs, data);
    return true;
  } catch (const std::exception& e) {
    return false;
  }
}

FOMDataUnion FOMDataSerializer::Read(RakNet::BitStream& bs,
                                     const PacketIdentifier id) {
  const auto* reader = GetReader(id);
  if (!reader) {
    throw ReadError(Packet::ReadPacketError{
        id, Packet::ReadPacketErrorCode::ERROR_UNHANDLED_PACKET_ID});
  }

  // Make sure to catch any deserialization errors so that
  // the library does not crash the consuming application.
  try {
    return reader->Read(bs);
  } catch (const std::exception& e) {
    throw ReadError(
        Packet::ReadPacketError{id, Packet::ReadPacketErrorCode::ERROR_READ});
  }
}

const IWriter* FOMDataSerializer::GetWriter(PacketIdentifier id) {
  auto it = writerMap.find(id);
  if (it == writerMap.end()) {
    return NULL;
  }
  return it->second;
}

const IReader* FOMDataSerializer::GetReader(PacketIdentifier id) {
  auto it = readerMap.find(id);
  if (it == readerMap.end()) {
    return NULL;
  }
  return it->second;
}

}  // namespace FOMNetwork
