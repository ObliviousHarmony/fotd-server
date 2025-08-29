#include <fom-network/ClientAPI.h>

RakPeerInterface* FOMNetwork_Client_Connect(const uint8_t* hostAddress, uint32_t hostAddressLen, uint16_t port) {
	if (!hostAddress || hostAddressLen == 0 || port == 0)
		return 0;

	return 0;
}

void FOMNetwork_Client_Disconnect(RakPeerInterface* peer) {
	if (!peer)
		return;
}
