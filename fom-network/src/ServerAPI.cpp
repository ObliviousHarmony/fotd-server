#include <raknet/RakNetworkFactory.h>
#include <fom-network/ServerAPI.h>

RakPeerInterface* FOMNetwork_Server_Startup(uint16_t port) {
	if (!port) {
		return NULL;
	}

	RakPeerInterface* server = RakNetworkFactory::GetRakPeerInterface();
	if (!server) {
		return NULL;
	}

	SocketDescriptor sd(port, 0);
	if (!server->Startup(1, 0, &sd, 1)) {
		RakNetworkFactory::DestroyRakPeerInterface(server);
		return NULL;
	}

	server->SetMaximumIncomingConnections(1000);

	return server;
}


void FOMNetwork_Server_Shutdown(RakPeerInterface* server) {
	if (!server) {
		return;
	}

	server->Shutdown(0, 0);
	RakNetworkFactory::DestroyRakPeerInterface(server);
}
