#include <fom-network/ServerApi.h>

#include "RakNetIncludes.h"

FOMNetworkPeer* FOMNetwork_Server_Startup(uint16_t port, uint32_t maxClients,
                                          int32_t threadSleepMs) {
  if (!port) {
    return NULL;
  }

  RakPeerInterface* server = RakNetworkFactory::GetRakPeerInterface();
  if (!server) {
    return NULL;
  }

  SocketDescriptor sd(port, 0);
  if (!server->Startup(maxClients, threadSleepMs, &sd, 1)) {
    RakNetworkFactory::DestroyRakPeerInterface(server);
    return NULL;
  }

  server->SetIncomingPassword("37eG87Ph", 8);
  server->SetMaximumIncomingConnections(1000);

  return static_cast<FOMNetworkPeer*>(server);
}

void FOMNetwork_Server_Shutdown(FOMNetworkPeer* server) {
  auto rakPeer = static_cast<RakPeerInterface*>(server);
  if (!rakPeer) {
    return;
  }

  rakPeer->Shutdown(1000, 0);
  RakNetworkFactory::DestroyRakPeerInterface(rakPeer);
}
