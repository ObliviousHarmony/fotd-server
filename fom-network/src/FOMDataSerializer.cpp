#include <fom-network/FOMDataSerializer.h>

#include <unordered_map>

namespace FOMNetwork {

/**
 * A map of all of the packets that the serializer can handle and their
 * associated sizes.
 */
const std::unordered_map<uint8_t, uint32_t> FOMDataSerializer::PacketSizes = {
    {ID_FOM_PACKET_READ_ERROR, sizeof(Packet::ReadPacketError)},

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
    {ID_LOGIN_REQUEST, sizeof(Packet::LoginRequest)},
    {ID_LOGIN_REQUEST_RETURN, sizeof(Packet::LoginRequestReturn)},
    {ID_LOGIN, sizeof(Packet::Login)},
    {ID_LOGIN_RETURN, sizeof(Packet::LoginReturn)},
    {ID_CHECK_NAME, sizeof(Packet::CheckName)},
    {ID_CHECK_NAME_RETURN, sizeof(Packet::CheckNameReturn)},
    {ID_CREATE_CHARACTER, sizeof(Packet::CreateCharacter)},
    {ID_REGISTER_WORLD, sizeof(Packet::RegisterWorld)},
    {ID_WORLD_OVERVIEW, sizeof(Packet::WorldOverview)},
    {ID_WORLD_OVERVIEW_RETURN, sizeof(Packet::WorldOverviewReturn)},
    {ID_WORLD_LOGIN, sizeof(Packet::WorldLogin)},
    {ID_WORLD_LOGIN_RETURN, sizeof(Packet::WorldLoginReturn)},
    {ID_PLAYER_ENTERING_WORLD, sizeof(Packet::PlayerEnteringWorld)},
    {ID_PLAYER_ENTERING_WORLD_RETURN,
     sizeof(Packet::PlayerEnteringWorldReturn)},
};

/**
 * We need to initialize the map with all of the serializers we want to be able
 * to use.
 */
static const std::unordered_map<uint32_t, IWriter*> writerMap = {
    {ID_LOGIN_REQUEST_RETURN, &LoginRequestReturnSerializer::GetInstance()},
    {ID_LOGIN_RETURN, &LoginReturnSerializer::GetInstance()},
    {ID_CHECK_NAME_RETURN, &CheckNameReturnSerializer::GetInstance()},
    {ID_REGISTER_WORLD, &RegisterWorldSerializer::GetInstance()},
    {ID_WORLD_OVERVIEW_RETURN, &WorldOverviewReturnSerializer::GetInstance()},
    {ID_WORLD_LOGIN_RETURN, &WorldLoginReturnSerializer::GetInstance()},
    {ID_PLAYER_ENTERING_WORLD, &PlayerEnteringWorldSerializer::GetInstance()},
    {ID_PLAYER_ENTERING_WORLD_RETURN,
     &PlayerEnteringWorldReturnSerializer::GetInstance()},
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
    {ID_LOGIN_REQUEST, &LoginRequestSerializer::GetInstance()},
    {ID_LOGIN, &LoginSerializer::GetInstance()},
    {ID_CHECK_NAME, &CheckNameSerializer::GetInstance()},
    {ID_CREATE_CHARACTER, &CreateCharacterSerializer::GetInstance()},
    {ID_REGISTER_WORLD, &RegisterWorldSerializer::GetInstance()},
    {ID_WORLD_OVERVIEW, &WorldOverviewSerializer::GetInstance()},
    {ID_WORLD_LOGIN, &WorldLoginSerializer::GetInstance()},
    {ID_PLAYER_ENTERING_WORLD, &PlayerEnteringWorldSerializer::GetInstance()},
    {ID_PLAYER_ENTERING_WORLD_RETURN,
     &PlayerEnteringWorldReturnSerializer::GetInstance()},
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

bool FOMDataSerializer::Read(RakNet::BitStream& bs, const PacketIdentifier id,
                             uint8_t* dataBuffer) {
  const auto* reader = GetReader(id);
  if (!reader) {
    Packet::ReadPacketError* data =
        reinterpret_cast<Packet::ReadPacketError*>(dataBuffer);
    data->offendingID = id;
    data->errorCode = Packet::ReadPacketErrorCode::ERROR_UNHANDLED_PACKET_ID;
    return true;
  }

  // Make sure to catch any deserialization errors so that
  // the library does not crash the consuming application.
  try {
    bool ret = reader->Read(bs, dataBuffer);
    if (!ret) {
      Packet::ReadPacketError* data =
          reinterpret_cast<Packet::ReadPacketError*>(dataBuffer);
      data->offendingID = id;
      data->errorCode = Packet::ReadPacketErrorCode::ERROR_READ;
      return true;
    }

    return ret;
  } catch (const std::exception& e) {
    Packet::ReadPacketError* data =
        reinterpret_cast<Packet::ReadPacketError*>(dataBuffer);
    data->offendingID = id;
    data->errorCode = Packet::ReadPacketErrorCode::ERROR_READ;
    return true;
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

int GetPacketSize(FOMNetwork::PacketIdentifier id) {
  auto it = FOMDataSerializer::PacketSizes.find(id);
  if (it == FOMDataSerializer::PacketSizes.end()) {
    return -1;
  }
  return it->second;
}

}  // namespace FOMNetwork
