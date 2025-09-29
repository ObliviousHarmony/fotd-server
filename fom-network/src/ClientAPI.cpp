#include <fom-network/ClientAPI.h>
#include <raknet/MessageIdentifiers.h>
#include <raknet/RakNetworkFactory.h>
#include <raknet/RakSleep.h>

RakPeerInterface* FOMNetwork_Client_Connect(const char* hostAddress,
                                            uint16_t port) {
  if (!hostAddress || port == 0) {
    return NULL;
  }

  RakPeerInterface* client = RakNetworkFactory::GetRakPeerInterface();
  if (!client) {
    return NULL;
  }

  SocketDescriptor sd{};
  if (!client->Startup(1, 0, &sd, 1)) {
    RakNetworkFactory::DestroyRakPeerInterface(client);
    return NULL;
  }

  if (!client->Connect(hostAddress, port, "37eG87Ph", 8)) {
    client->Shutdown(0, 0);
    RakNetworkFactory::DestroyRakPeerInterface(client);
    return NULL;
  }

  // Consumers of this API expect to receive a peer that is ready to start
  // communicating with the given host. Since RakNet is asynchronous,
  // we need to wait until we get confirmation of whether or not a
  // connection could be established.
  bool connected;
  while (true) {
    // Block until RakNet provides the connection result packet.
    Packet* packet = client->Receive();
    if (!packet) {
      RakSleep(50);
      continue;
    }

    switch (packet->data[0]) {
      case ID_CONNECTION_REQUEST_ACCEPTED:
        connected = true;
        break;

      case ID_NO_FREE_INCOMING_CONNECTIONS:
      case ID_CONNECTION_BANNED:
      case ID_INVALID_PASSWORD:
      case ID_ALREADY_CONNECTED:
      case ID_RSA_PUBLIC_KEY_MISMATCH:
      case ID_CONNECTION_ATTEMPT_FAILED:
        connected = false;
        break;
    }

    client->DeallocatePacket(packet);
    break;
  }

  if (!connected) {
    client->Shutdown(0, 0);
    RakNetworkFactory::DestroyRakPeerInterface(client);
    return NULL;
  }

  return client;
}

void FOMNetwork_Client_Disconnect(RakPeerInterface* client) {
  if (!client) {
    return;
  }

  client->Shutdown(0, 0);
  RakNetworkFactory::DestroyRakPeerInterface(client);
}
