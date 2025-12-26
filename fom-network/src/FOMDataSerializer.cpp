#include <fom-network/FOMDataSerializer.h>

#include <unordered_map>

namespace FOMNetwork {

/**
 * A map of all of the packets that the serializer can handle and their
 * associated sizes.
 */
const std::unordered_map<uint8_t, size_t> FOMDataSerializer::PacketSizes = {
    // RakNet Packets
    {ID_ALREADY_CONNECTED, sizeof(Packet::AlreadyConnected)},
    {ID_CONNECTION_ATTEMPT_FAILED, sizeof(Packet::ConnectionAttemptFailed)},
    {ID_CONNECTION_BANNED, sizeof(Packet::ConnectionBanned)},
    {ID_CONNECTION_LOST, sizeof(Packet::ConnectionLost)},
    {ID_CONNECTION_REQUEST_ACCEPTED, sizeof(Packet::ConnectionRequestAccepted)},
    {ID_DISCONNECTION_NOTIFICATION, sizeof(Packet::DisconnectionNotification)},
    {ID_INVALID_PASSWORD, sizeof(Packet::InvalidPassword)},
    {ID_MODIFIED_PACKET, sizeof(Packet::ModifiedPacket)},
    {ID_NEW_INCOMING_CONNECTION, sizeof(Packet::NewIncomingConnection)},
    {ID_NO_FREE_INCOMING_CONNECTIONS,
     sizeof(Packet::NoFreeIncomingConnections)},
    {ID_RSA_PUBLIC_KEY_MISMATCH, sizeof(Packet::RSAPublicKeyMismatch)},

    // Game Packets
    {ID_FOM_PACKET_READ_ERROR, sizeof(Packet::ReadPacketError)},
    {ID_REGISTER_WORLD, sizeof(Packet::RegisterWorld)},
    {ID_LOGIN_REQUEST, sizeof(Packet::LoginRequest)},
    {ID_LOGIN_REQUEST_RETURN, sizeof(Packet::LoginRequestReturn)},
    {ID_LOGIN, sizeof(Packet::Login)},
    {ID_LOGIN_TOKEN_CHECK, sizeof(Packet::LoginTokenCheck)},
};

/**
 * We need to initialize the map with all of the serializers we want to be able
 * to use.
 */
static const std::unordered_map<uint32_t, IWriter*> writerMap = {
    {ID_REGISTER_WORLD, &RegisterWorldSerializer::GetInstance()},
    {ID_LOGIN_REQUEST_RETURN, &LoginRequestReturnSerializer::GetInstance()},
    {ID_LOGIN_TOKEN_CHECK, &LoginTokenCheckSerializer::GetInstance()},
};

static const std::unordered_map<uint32_t, IReader*> readerMap = {
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
    {ID_REGISTER_WORLD, &RegisterWorldSerializer::GetInstance()},
    {ID_LOGIN_REQUEST, &LoginRequestSerializer::GetInstance()},
    {ID_LOGIN, &LoginSerializer::GetInstance()},
    {ID_LOGIN_TOKEN_CHECK, &LoginTokenCheckSerializer::GetInstance()},
};

bool FOMDataSerializer::Write(RakNet::BitStream& bs, const PacketIdentifier id,
                              const uint8_t* data) {
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

bool FOMDataSerializer::Read(RakNet::BitStream& bs, const PacketIdentifier id,
                             uint8_t* dataBuffer) {
  const auto* reader = GetReader(id);
  if (!reader) {
    return false;
  }

  // Make sure to catch any deserialization errors so that
  // the library does not crash the consuming application.
  try {
    return reader->Read(bs, dataBuffer);
  } catch (const std::exception& e) {
    return false;
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

int FOMDataSerializer::GetPacketSize(FOMNetwork::PacketIdentifier id) {
  auto it = PacketSizes.find(id);
  if (it == PacketSizes.end()) {
    return -1;
  }
  return (int)it->second;
}

}  // namespace FOMNetwork
