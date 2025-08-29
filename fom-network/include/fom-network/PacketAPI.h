#pragma once

#include <raknet/RakNetTypes.h>
#include <raknet/RakPeer.h>
#include <fom-network/Common.h>
#include <fom-network/FOMNetworkExport.h>
#include <fom-network/FOMPacket.h>

#pragma pack(push, 1)

/**
* Container for passing a buffer of received packets around.
*
* @note This structure MUST only contain C# blittable types.
*/
struct ReceivedPackets {
	/* A buffer of packets that have been received and need to be deserialized and released. */
	Packet* packets;

	/* The number of packets in the buffer. */
	uint32_t count;
};

/*
* The network address for a system.
*
* @note This structure MUST only contain C# blittable types.
*/
struct FOMNetworkAddress {
	/* The binary destination address for the packet or the excluded address if it's broadcasted. */
	uint32_t binaryAddress;

	/* The destination port for the packet or the excluded address if it's broadcasted. */
	uint16_t port;
};


/**
* Container for passing packets to be sent around.
*
* @note This structure MUST only contain C# blittable types.
*/
struct SendPacket {
	/* The destination for the packet or the excluded address if it is a broadcast. */
	FOMNetworkAddress address;

	/* The discriminated union for communicating packet data. */
	FOMPacket data;

	/* The priority of the packet to be sent to the networking library. */
	int32_t priority;

	/* The reliability of the packet to be sent to the networking library. */
	int32_t reliability;

	/* A boolean indicating whether or not the packet should be a broadcast. */
	int8_t broadcast;
};

#pragma pack(pop)

#ifdef __cplusplus
extern "C" {
#endif

	/*
	* Polls the network interface for packets and returns them in a buffer.
	*
	* @param peer A pointer to the network interface.
	* 
	* @return A structure containing a buffer of received packets and the number of packets in the buffer.
	*/
	FOM_API ReceivedPackets FOMNetwork_ReceivePackets(RakPeerInterface* peer);

	/*
	* Uses the received packets to fill a buffer with deserialized packet structures.
	*
	* @param peer A pointer to the network interface.
	* @param received A structure containing a buffer of received packets and the number of packets in
	* @param packetBuffer A buffer to be filled with deserialized packet structures.
	* @param packetBufferLen The number of packets in the packet buffer.
	*
	* @return The number of packets that were deserialized and placed in the packet buffer.
	*/
	FOM_API uint32_t FOMNetwork_ProcessPackets(RakPeerInterface* peer, ReceivedPackets received, FOMPacket* packetBuffer, uint32_t packetBufferLen);

	/*
	* Sends a buffer of packet structures through the network interface.
	*
	* @param peer A pointer to the network interface.
	* @param packets A buffer of packet structures to serialize and send.
	* @param count The number of packets in the buffer.
	*/
	FOM_API void FOMNetwork_Send(RakPeerInterface* peer, SendPacket* packets, uint32_t count);

#ifdef __cplusplus
}
#endif
