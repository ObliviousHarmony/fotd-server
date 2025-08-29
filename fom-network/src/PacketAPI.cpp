#include <fom-network/PacketAPI.h>


ReceivedPackets FOMNetwork_ReceivePackets(RakPeerInterface* peer) {
	if (!peer)
		return { NULL, 0 };

	return { NULL, 0 };
}


uint32_t FOMNetwork_ProcessPackets(RakPeerInterface* peer, ReceivedPackets received, FOMPacket* packetBuffer, uint32_t packetBufferLen) {
	if (!peer || !received.packets || received.count == 0 || !packetBuffer || packetBufferLen == 0)
		return 0;

	return 0;
}


void FOMNetwork_Send(RakPeerInterface* peer, SendPacket* packets, uint32_t count) {
	if (!peer || !packets || count == 0)
		return;
}
