#include "FOMDataSerializer.h"

#include <fom-network/packets/CheckName.h>
#include <fom-network/packets/CheckNameReturn.h>
#include <fom-network/packets/CreateCharacter.h>
#include <fom-network/packets/Login.h>
#include <fom-network/packets/LoginRequest.h>
#include <fom-network/packets/LoginRequestReturn.h>
#include <fom-network/packets/LoginReturn.h>
#include <fom-network/packets/LoginTokenCheck.h>
#include <fom-network/packets/PlayerLeavingWorld.h>
#include <fom-network/packets/PlayerMigrateWorld.h>
#include <fom-network/packets/PlayerWorldReady.h>
#include <fom-network/packets/RegisterClient.h>
#include <fom-network/packets/RegisterClientReturn.h>
#include <fom-network/packets/RegisterWorld.h>
#include <fom-network/packets/WorldLogin.h>
#include <fom-network/packets/WorldLoginReturn.h>
#include <fom-network/packets/raknet/AlreadyConnected.h>
#include <fom-network/packets/raknet/ConnectionAttemptFailed.h>
#include <fom-network/packets/raknet/ConnectionBanned.h>
#include <fom-network/packets/raknet/ConnectionLost.h>
#include <fom-network/packets/raknet/ConnectionRequestAccepted.h>
#include <fom-network/packets/raknet/DisconnectionNotification.h>
#include <fom-network/packets/raknet/InvalidPassword.h>
#include <fom-network/packets/raknet/ModifiedPacket.h>
#include <fom-network/packets/raknet/NewIncomingConnection.h>
#include <fom-network/packets/raknet/NoFreeIncomingConnections.h>
#include <fom-network/packets/raknet/RsaPublicKeyMismatch.h>

#include <unordered_map>

namespace FOMNetwork {

/**
 * A map of all of the packets that the serializer can handle and their
 * associated sizes.
 */
static const std::unordered_map<uint8_t, size_t> packetSizes = {
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
    {ID_RSA_PUBLIC_KEY_MISMATCH, sizeof(Packet::RsaPublicKeyMismatch)},

    // Game Packets
    {Enum::ID_REGISTER_WORLD, sizeof(Packet::RegisterWorld)},
    {Enum::ID_LOGIN_REQUEST, sizeof(Packet::LoginRequest)},
    {Enum::ID_LOGIN_REQUEST_RETURN, sizeof(Packet::LoginRequestReturn)},
    {Enum::ID_LOGIN, sizeof(Packet::Login)},
    {Enum::ID_LOGIN_TOKEN_CHECK, sizeof(Packet::LoginTokenCheck)},
    {Enum::ID_CHECK_NAME, sizeof(Packet::CheckName)},
    {Enum::ID_CHECK_NAME_RETURN, sizeof(Packet::CheckNameReturn)},
    {Enum::ID_CREATE_CHARACTER, sizeof(Packet::CreateCharacter)},
    {Enum::ID_LOGIN_RETURN, sizeof(Packet::LoginReturn)},
    {Enum::ID_WORLD_LOGIN, sizeof(Packet::WorldLogin)},
    {Enum::ID_WORLD_LOGIN_RETURN, sizeof(Packet::WorldLoginReturn)},
    {Enum::ID_PLAYER_MIGRATE_WORLD, sizeof(Packet::PlayerMigrateWorld)},
    {Enum::ID_PLAYER_WORLD_READY, sizeof(Packet::PlayerWorldReady)},
    {Enum::ID_PLAYER_LEAVING_WORLD, sizeof(Packet::PlayerLeavingWorld)},
    {Enum::ID_REGISTER_CLIENT, sizeof(Packet::RegisterClient)},
    {Enum::ID_REGISTER_CLIENT_RETURN, sizeof(Packet::RegisterClientReturn)},
};

/**
 * We need to initialize the map with all of the serializers we want to be able
 * to use.
 */
static const std::unordered_map<uint32_t, IWriter*> writerMap = {
    {Enum::ID_REGISTER_WORLD, &RegisterWorldSerializer::GetInstance()},
    {Enum::ID_LOGIN_REQUEST_RETURN,
     &LoginRequestReturnSerializer::GetInstance()},
    {Enum::ID_LOGIN_TOKEN_CHECK, &LoginTokenCheckSerializer::GetInstance()},
    {Enum::ID_CHECK_NAME_RETURN, &CheckNameReturnSerializer::GetInstance()},
    {Enum::ID_LOGIN_RETURN, &LoginReturnSerializer::GetInstance()},
    {Enum::ID_WORLD_LOGIN_RETURN, &WorldLoginReturnSerializer::GetInstance()},
    {Enum::ID_PLAYER_MIGRATE_WORLD,
     &PlayerMigrateWorldSerializer::GetInstance()},
    {Enum::ID_PLAYER_WORLD_READY, &PlayerWorldReadySerializer::GetInstance()},
    {Enum::ID_PLAYER_LEAVING_WORLD,
     &PlayerLeavingWorldSerializer::GetInstance()},
    {Enum::ID_REGISTER_CLIENT_RETURN,
     &RegisterClientReturnSerializer::GetInstance()},
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
    {Enum::ID_REGISTER_WORLD, &RegisterWorldSerializer::GetInstance()},
    {Enum::ID_LOGIN_REQUEST, &LoginRequestSerializer::GetInstance()},
    {Enum::ID_LOGIN, &LoginSerializer::GetInstance()},
    {Enum::ID_LOGIN_TOKEN_CHECK, &LoginTokenCheckSerializer::GetInstance()},
    {Enum::ID_CHECK_NAME, &CheckNameSerializer::GetInstance()},
    {Enum::ID_CREATE_CHARACTER, &CreateCharacterSerializer::GetInstance()},
    {Enum::ID_WORLD_LOGIN, &WorldLoginSerializer::GetInstance()},
    {Enum::ID_PLAYER_MIGRATE_WORLD,
     &PlayerMigrateWorldSerializer::GetInstance()},
    {Enum::ID_PLAYER_WORLD_READY, &PlayerWorldReadySerializer::GetInstance()},
    {Enum::ID_PLAYER_LEAVING_WORLD,
     &PlayerLeavingWorldSerializer::GetInstance()},
    {Enum::ID_REGISTER_CLIENT, &RegisterClientSerializer::GetInstance()},
};

bool FOMDataSerializer::Write(RakNet::BitStream& bs,
                              const Enum::PacketIdentifier id,
                              const uint8_t* data) {
  const auto* writer = GetWriter(id);
  if (!writer) {
    return false;
  }

  // Make sure to catch any serialization error so that the
  // library does not crash the consuming application.
  try {
    writer->WriteRaw(bs, data);
    return true;
  } catch (const std::exception& e) {
    return false;
  }
}

bool FOMDataSerializer::Read(RakNet::BitStream& bs,
                             const Enum::PacketIdentifier id,
                             uint8_t* dataBuffer) {
  const auto* reader = GetReader(id);
  if (!reader) {
    return false;
  }

  // Make sure to catch any deserialization errors so that
  // the library does not crash the consuming application.
  try {
    return reader->ReadRaw(bs, dataBuffer);
  } catch (const std::exception& e) {
    return false;
  }
}

const IWriter* FOMDataSerializer::GetWriter(Enum::PacketIdentifier id) {
  auto it = writerMap.find(id);
  if (it == writerMap.end()) {
    return NULL;
  }
  return it->second;
}

const IReader* FOMDataSerializer::GetReader(Enum::PacketIdentifier id) {
  auto it = readerMap.find(id);
  if (it == readerMap.end()) {
    return NULL;
  }
  return it->second;
}

size_t FOMDataSerializer::GetPacketCount() { return packetSizes.size(); }

int FOMDataSerializer::GetPacketSize(Enum::PacketIdentifier id) {
  auto it = packetSizes.find(id);
  if (it == packetSizes.end()) {
    return -1;
  }
  return (int)it->second;
}

}  // namespace FOMNetwork
