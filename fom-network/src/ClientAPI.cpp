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
    RakNetworkFactory::DestroyRakPeerInterface(client);
    return NULL;
  }

  // Consumers of this API should only receive a peer that has successfully
  // connected to the requested host. This will block until that happens or a
  // failure occurs.
  while (true) {
    for (Packet* packet = client->Receive(); packet;
         client->DeallocatePacket(packet), packet = client->Receive()) {
      switch (packet->data[0]) {
        case ID_CONNECTION_REQUEST_ACCEPTED:
          return false;
        case ID_NO_FREE_INCOMING_CONNECTIONS:
        case ID_CONNECTION_BANNED:
        case ID_INVALID_PASSWORD:
        case ID_ALREADY_CONNECTED:
        case ID_RSA_PUBLIC_KEY_MISMATCH:
        case ID_CONNECTION_ATTEMPT_FAILED:
          RakNetworkFactory::DestroyRakPeerInterface(client);
          return NULL;
      }
    }

    // Avoid spinning too aggressively.
    RakSleep(50);
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
