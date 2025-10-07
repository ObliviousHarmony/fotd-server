#include <fom-network/NetworkAPI.h>
#include <fom-network/packets/PacketTypes.h>

#include <unordered_map>
#include <unordered_set>

using namespace FOMNetwork;
using namespace FOMNetwork::Packet;

int32_t FOMNetwork_ValidatePacketStructs(const PacketStructure* structures,
                                         int32_t count) {
#pragma warning(push)
#pragma warning(disable : 4267)

  // List all of the structs that we have defined in the library
  // so that they can be compared to the consumer's structs.
  std::unordered_map<uint8_t, uint32_t> libraryMap = {
      {ID_FOM_PACKET_READ_ERROR, sizeof(ReadPacketError)},

      // RakNet Packets
      {ID_ALREADY_CONNECTED, sizeof(AlreadyConnected)},
      {ID_CONNECTION_ATTEMPT_FAILED, sizeof(ConnectionAttemptFailed)},
      {ID_CONNECTION_BANNED, sizeof(ConnectionBanned)},
      {ID_CONNECTION_LOST, sizeof(ConnectionLost)},
      {ID_CONNECTION_REQUEST_ACCEPTED, sizeof(ConnectionRequestAccepted)},
      {ID_DISCONNECTION_NOTIFICATION, sizeof(DisconnectionNotification)},
      {ID_INVALID_PASSWORD, sizeof(InvalidPassword)},
      {ID_MODIFIED_PACKET, sizeof(ModifiedPacket)},
      {ID_NEW_INCOMING_CONNECTION, sizeof(NewIncomingConnection)},
      {ID_NO_FREE_INCOMING_CONNECTIONS, sizeof(NoFreeIncomingConnections)},
      {ID_RSA_PUBLIC_KEY_MISMATCH, sizeof(RSAPublicKeyMismatch)},

      // Game Packets
      {ID_FOM_PACKET_READ_ERROR, sizeof(ReadPacketError)},
      {ID_LOGIN_REQUEST, sizeof(LoginRequest)},
      {ID_LOGIN_REQUEST_RETURN, sizeof(LoginRequestReturn)},
      {ID_LOGIN, sizeof(Login)},
      {ID_LOGIN_RETURN, sizeof(LoginReturn)},
      {ID_CHECK_NAME, sizeof(CheckName)},
      {ID_CHECK_NAME_RETURN, sizeof(CheckNameReturn)},
      {ID_CREATE_CHARACTER, sizeof(CreateCharacter)},
      {ID_REGISTER_WORLD, sizeof(RegisterWorld)},
      {ID_WORLD_OVERVIEW, sizeof(WorldOverview)},
      {ID_WORLD_OVERVIEW_RETURN, sizeof(WorldOverviewReturn)},
      {ID_WORLD_LOGIN, sizeof(WorldLogin)},
      {ID_WORLD_LOGIN_RETURN, sizeof(WorldLoginReturn)},
      {ID_PLAYER_ENTERING_WORLD, sizeof(PlayerEnteringWorld)},
      {ID_PLAYER_ENTERING_WORLD_RETURN, sizeof(PlayerEnteringWorldReturn)},
  };

#pragma warning(pop)

  // Both should have the same number of packets defined.
  if (libraryMap.size() != count) {
    return -1;
  }

  // Make sure that the size of each packet ID matches.
  // This is not a comprehensive check but the
  // consumer can perform any additional
  // verification that may be needed.
  for (int32_t i = 0; i < count; i++) {
    auto it = libraryMap.find(structures[i].id);
    if (it == libraryMap.end()) {
      return -2;
    }

    if (it->second != structures[i].size) {
      return -3;
    }
  }

  return 0;
}
