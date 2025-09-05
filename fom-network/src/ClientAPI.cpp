#include <raknet/RakNetworkFactory.h>
#include <fom-network/ClientAPI.h>

RakPeerInterface* FOMNetwork_Client_Connect(const char* hostAddress, uint16_t port) {
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

	if (!client->Connect(hostAddress, port, 0, 0)) {
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
