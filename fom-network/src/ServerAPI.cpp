#include <fom-network/ServerAPI.h>


RakPeerInterface* FOMNetwork_Server_Startup(uint16_t port) {
	if (!port)
		return NULL;

	return NULL;
}


void FOMNetwork_Server_Shutdown(RakPeerInterface* peer) {
	if (!peer)
		return;
}
