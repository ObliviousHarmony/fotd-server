#include "FOMDataSerializer.h"

#include <fom-network/packets/ChatPacket.h>
#include <fom-network/packets/CheckNamePacket.h>
#include <fom-network/packets/CheckNameReturnPacket.h>
#include <fom-network/packets/CreateCharacterPacket.h>
#include <fom-network/packets/LoginPacket.h>
#include <fom-network/packets/LoginRequestPacket.h>
#include <fom-network/packets/LoginRequestReturnPacket.h>
#include <fom-network/packets/LoginReturnPacket.h>
#include <fom-network/packets/LoginTokenCheckPacket.h>
#include <fom-network/packets/MoveItemsPacket.h>
#include <fom-network/packets/PlayerLeavingWorldPacket.h>
#include <fom-network/packets/PlayerMigrateWorldPacket.h>
#include <fom-network/packets/PlayerWorldReadyPacket.h>
#include <fom-network/packets/RegisterClientPacket.h>
#include <fom-network/packets/RegisterClientReturnPacket.h>
#include <fom-network/packets/RegisterWorldPacket.h>
#include <fom-network/packets/UpdatePacket.h>
#include <fom-network/packets/WorldLoginPacket.h>
#include <fom-network/packets/WorldLoginReturnPacket.h>
#include <fom-network/packets/WorldObjectsPacket.h>
#include <fom-network/packets/WorldServicePacket.h>
#include <fom-network/packets/WorldUpdatePacket.h>
#include <fom-network/packets/raknet/AlreadyConnectedPacket.h>
#include <fom-network/packets/raknet/ConnectionAttemptFailedPacket.h>
#include <fom-network/packets/raknet/ConnectionBannedPacket.h>
#include <fom-network/packets/raknet/ConnectionLostPacket.h>
#include <fom-network/packets/raknet/ConnectionRequestAcceptedPacket.h>
#include <fom-network/packets/raknet/DisconnectionNotificationPacket.h>
#include <fom-network/packets/raknet/InvalidPasswordPacket.h>
#include <fom-network/packets/raknet/ModifiedPacketPacket.h>
#include <fom-network/packets/raknet/NewIncomingConnectionPacket.h>
#include <fom-network/packets/raknet/NoFreeIncomingConnectionsPacket.h>
#include <fom-network/packets/raknet/RsaPublicKeyMismatchPacket.h>

#include <unordered_map>

namespace FOMNetwork {

/**
 * A map of all of the packets that the serializer can handle and their
 * associated sizes.
 */
static const std::unordered_map<uint8_t, size_t> packetSizes = {
    // RakNet Packets
    {ID_ALREADY_CONNECTED, sizeof(AlreadyConnectedPacket)},
    {ID_CONNECTION_ATTEMPT_FAILED, sizeof(ConnectionAttemptFailedPacket)},
    {ID_CONNECTION_BANNED, sizeof(ConnectionBannedPacket)},
    {ID_CONNECTION_LOST, sizeof(ConnectionLostPacket)},
    {ID_CONNECTION_REQUEST_ACCEPTED, sizeof(ConnectionRequestAcceptedPacket)},
    {ID_DISCONNECTION_NOTIFICATION, sizeof(DisconnectionNotificationPacket)},
    {ID_INVALID_PASSWORD, sizeof(InvalidPasswordPacket)},
    {ID_MODIFIED_PACKET, sizeof(ModifiedPacketPacket)},
    {ID_NEW_INCOMING_CONNECTION, sizeof(NewIncomingConnectionPacket)},
    {ID_NO_FREE_INCOMING_CONNECTIONS, sizeof(NoFreeIncomingConnectionsPacket)},
    {ID_RSA_PUBLIC_KEY_MISMATCH, sizeof(RsaPublicKeyMismatchPacket)},

    // Game Packets
    {Enum::ID_REGISTER_WORLD, sizeof(RegisterWorldPacket)},
    {Enum::ID_LOGIN_REQUEST, sizeof(LoginRequestPacket)},
    {Enum::ID_LOGIN_REQUEST_RETURN, sizeof(LoginRequestReturnPacket)},
    {Enum::ID_LOGIN, sizeof(LoginPacket)},
    {Enum::ID_LOGIN_TOKEN_CHECK, sizeof(LoginTokenCheckPacket)},
    {Enum::ID_CHECK_NAME, sizeof(CheckNamePacket)},
    {Enum::ID_CHECK_NAME_RETURN, sizeof(CheckNameReturnPacket)},
    {Enum::ID_CREATE_CHARACTER, sizeof(CreateCharacterPacket)},
    {Enum::ID_LOGIN_RETURN, sizeof(LoginReturnPacket)},
    {Enum::ID_WORLD_LOGIN, sizeof(WorldLoginPacket)},
    {Enum::ID_WORLD_LOGIN_RETURN, sizeof(WorldLoginReturnPacket)},
    {Enum::ID_PLAYER_MIGRATE_WORLD, sizeof(PlayerMigrateWorldPacket)},
    {Enum::ID_PLAYER_WORLD_READY, sizeof(PlayerWorldReadyPacket)},
    {Enum::ID_PLAYER_LEAVING_WORLD, sizeof(PlayerLeavingWorldPacket)},
    {Enum::ID_REGISTER_CLIENT, sizeof(RegisterClientPacket)},
    {Enum::ID_REGISTER_CLIENT_RETURN, sizeof(RegisterClientReturnPacket)},
    {Enum::ID_UPDATE, sizeof(UpdatePacket)},
    {Enum::ID_WORLD_UPDATE, sizeof(WorldUpdatePacket)},
    {Enum::ID_CHAT, sizeof(ChatPacket)},
    {Enum::ID_MOVE_ITEMS, sizeof(MoveItemsPacket)},
    {Enum::ID_WORLD_OBJECTS, sizeof(WorldObjectsPacket)},
    {Enum::ID_WORLDSERVICE, sizeof(WorldServicePacket)},
};

/**
 * We need to initialize the map with all of the serializers we want to be able
 * to use.
 */
static const std::unordered_map<uint32_t, IWriter*> writerMap = {
    {Enum::ID_REGISTER_WORLD, &RegisterWorldPacketSerializer::GetInstance()},
    {Enum::ID_LOGIN_REQUEST_RETURN,
     &LoginRequestReturnPacketSerializer::GetInstance()},
    {Enum::ID_LOGIN_TOKEN_CHECK,
     &LoginTokenCheckPacketSerializer::GetInstance()},
    {Enum::ID_CHECK_NAME_RETURN,
     &CheckNameReturnPacketSerializer::GetInstance()},
    {Enum::ID_LOGIN_RETURN, &LoginReturnPacketSerializer::GetInstance()},
    {Enum::ID_WORLD_LOGIN_RETURN,
     &WorldLoginReturnPacketSerializer::GetInstance()},
    {Enum::ID_PLAYER_MIGRATE_WORLD,
     &PlayerMigrateWorldPacketSerializer::GetInstance()},
    {Enum::ID_PLAYER_WORLD_READY,
     &PlayerWorldReadyPacketSerializer::GetInstance()},
    {Enum::ID_PLAYER_LEAVING_WORLD,
     &PlayerLeavingWorldPacketSerializer::GetInstance()},
    {Enum::ID_REGISTER_CLIENT_RETURN,
     &RegisterClientReturnPacketSerializer::GetInstance()},
    {Enum::ID_WORLD_UPDATE, &WorldUpdatePacketSerializer::GetInstance()},
    {Enum::ID_CHAT, &ChatPacketSerializer::GetInstance()},
    {Enum::ID_MOVE_ITEMS, &MoveItemsPacketSerializer::GetInstance()},
    {Enum::ID_WORLD_OBJECTS, &WorldObjectsPacketSerializer::GetInstance()},
    {Enum::ID_WORLDSERVICE, &WorldServicePacketSerializer::GetInstance()},
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
    {Enum::ID_REGISTER_WORLD, &RegisterWorldPacketSerializer::GetInstance()},
    {Enum::ID_LOGIN_REQUEST, &LoginRequestPacketSerializer::GetInstance()},
    {Enum::ID_LOGIN, &LoginPacketSerializer::GetInstance()},
    {Enum::ID_LOGIN_TOKEN_CHECK,
     &LoginTokenCheckPacketSerializer::GetInstance()},
    {Enum::ID_CHECK_NAME, &CheckNamePacketSerializer::GetInstance()},
    {Enum::ID_CREATE_CHARACTER,
     &CreateCharacterPacketSerializer::GetInstance()},
    {Enum::ID_WORLD_LOGIN, &WorldLoginPacketSerializer::GetInstance()},
    {Enum::ID_PLAYER_MIGRATE_WORLD,
     &PlayerMigrateWorldPacketSerializer::GetInstance()},
    {Enum::ID_PLAYER_WORLD_READY,
     &PlayerWorldReadyPacketSerializer::GetInstance()},
    {Enum::ID_PLAYER_LEAVING_WORLD,
     &PlayerLeavingWorldPacketSerializer::GetInstance()},
    {Enum::ID_REGISTER_CLIENT, &RegisterClientPacketSerializer::GetInstance()},
    {Enum::ID_UPDATE, &UpdatePacketSerializer::GetInstance()},
    {Enum::ID_CHAT, &ChatPacketSerializer::GetInstance()},
    {Enum::ID_MOVE_ITEMS, &MoveItemsPacketSerializer::GetInstance()},
    {Enum::ID_WORLDSERVICE, &WorldServicePacketSerializer::GetInstance()},
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
