#include <vector>
#include <fom-network/PacketAPI.h>
#include <fom-network/FOMPacketSerializer.h>

/**
 * The maximum number of packets that can be received each tick.
 * Since we buffer the packets to return to the consumer, we
 * don't want to stall the thread permanently when there are
 * more packets being received than pulled off the queue.
 */
namespace FOM {
	constexpr uint32_t MaxBufferedPackets = 256;
}

ReceivedPackets FOMNetwork_ReceivePackets(RakPeerInterface* peer) {
	if (!peer) {
		return { NULL, 0 };
	}

	// Make sure to re-use the same buffer since we are just using
	// it to aggregate the pointers before copying them to return.
	static std::vector<Packet*> receiveBuffer;
	receiveBuffer.clear();
	receiveBuffer.reserve(FOM::MaxBufferedPackets);

	// Limit the number of packets 
	while (receiveBuffer.size() < FOM::MaxBufferedPackets) {
		Packet* packet = peer->Receive();
		if (!packet) {
			break;
		}

		receiveBuffer.push_back(packet);
	}

	ReceivedPackets received{};

	// We're going to pass an array of packets back to the consumer.
	// The array and the packets contained must all be deallocated
	// in FOMNetwork_ProcessPackets so that we don't leak memory!
	received.count = receiveBuffer.size();
	received.packets = new Packet*[receiveBuffer.size()];
	std::copy(receiveBuffer.begin(), receiveBuffer.end(), received.packets);

	return received;
}

int8_t FOMNetwork_ProcessPackets(RakPeerInterface* peer, const ReceivedPackets received, FOMPacket* packetBuffer, uint32_t packetBufferLen) {
	if (!peer || !received.packets || received.count == 0) {
		return 0;
	}

	if (!packetBuffer || packetBufferLen != received.count) {
		return -1;
	}

	for (uint32_t i = 0; i < received.count; i++) {
		Packet* p = received.packets[i];
		if (!p) {
			continue;
		}

		RakNet::BitStream bs(p->data, p->length, false);
		FOMPacket deserializedPacket = FOMPacketSerializer::Deserialize(bs);

		// Make sure to record the packet's sender.
		deserializedPacket.sender.binaryAddress = p->systemAddress.binaryAddress;
		deserializedPacket.sender.port = p->systemAddress.port;

		packetBuffer[i] = deserializedPacket;

		// Release the packet back to RakNet.
		peer->DeallocatePacket(const_cast<Packet*>(p));
	}

	// Now that we're done we don't need the receive packet buffer anymore.
	delete[] received.packets;

	return 0;
}

void FOMNetwork_Send(RakPeerInterface* peer, const SendPacket* packets, uint32_t count) {
	if (!peer || !packets || count == 0) {
		return;
	}

	for (uint32_t i = 0; i < count; i++) {
		const SendPacket& s = packets[i];

		SystemAddress address = UNASSIGNED_SYSTEM_ADDRESS;
		if (s.destination.binaryAddress != 0 ) {
			address.binaryAddress = s.destination.binaryAddress;
			address.port = s.destination.port;
		}

		RakNet::BitStream bs;
		if (!FOMPacketSerializer::Serialize(bs, s.data)) {
			continue;
		}

		if (s.broadcast) {
			peer->Send(&bs, (PacketPriority)s.priority, (PacketReliability)s.reliability, s.orderingChannel, address, !!s.broadcast);
		} else {
			peer->Send(&bs, (PacketPriority)s.priority, (PacketReliability)s.reliability, s.orderingChannel, address, !!s.broadcast);
		}
	}
}
