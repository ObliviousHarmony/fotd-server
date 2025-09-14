#include <unordered_map>
#include <fom-network/FOMPacketSerializer.h>

/**
 * We need to initialize the map with all of the serializers we want to be able to use.
 */
static std::unordered_map<uint32_t, IPacketSerializer*> serializerMap = {
	//{ ID_FOM_PACKET_START, &ExamplePacketSerializer::GetInstance() }
};

bool FOMPacketSerializer::Serialize(RakNet::BitStream& bs, const FOMPacket& p) {
	const auto* serializer = GetSerializer(p.ID);
	if (!serializer) {
		return false;
	}

	// Make sure to catch any serialization error so that the
	// library does not crash the consuming application.
	try {
		// The first byte in the BitStream will always be the packet ID.
		bs.Write(p.ID);

		return serializer->SerializePacket(bs, p);
	} catch (const std::exception& e) {
		return false;
	}
}

FOMPacket FOMPacketSerializer::Deserialize(RakNet::BitStream& bs) {
	if (bs.GetNumberOfBitsUsed() < 8) {
		return FOMPacket{
			ID_FOM_PACKET_ERROR,
			{},
			{ FOMPacketError{ ID_FOM_PACKET_ERROR, FOMPacketErrorCode::ERROR_MISSING_PACKET_ID } }
		};
	}

	// The first byte in the BitStream will always be the packet ID.
	PacketIdentifier id;
	bs.Read((uint8_t&)id);

	// RakNet client packets should be forwarded with the ID so that the consumer
	// can handle these kinds of packets too.
	switch (id) {
		// Connection request to the server has been accepted.
		case ID_CONNECTION_REQUEST_ACCEPTED:
		// Could not connect to the server.
		case ID_CONNECTION_ATTEMPT_FAILED:
		// Attempted to connect to a server we're already connected to.
		case ID_ALREADY_CONNECTED:
		// Connection has been made successfully.
		case ID_NEW_INCOMING_CONNECTION:
		// Server is not accepting new connections.
		case ID_NO_FREE_INCOMING_CONNECTIONS:
		// The connected system has shut down.
		case ID_DISCONNECTION_NOTIFICATION:
		// The connection has been closed.
		case ID_CONNECTION_LOST:
		// Current RSA public key does not match what the destination expected.
		case ID_RSA_PUBLIC_KEY_MISMATCH:
		// Client is banned by the server.
		case ID_CONNECTION_BANNED:
		// Server has refused the client's passeord.
		case ID_INVALID_PASSWORD:
		// Packet has been tampered with in transit.
		case ID_MODIFIED_PACKET:
			return FOMPacket{ id };
	}

	const auto* serializer = GetSerializer(id);
	if (!serializer) {
		return FOMPacket{
			ID_FOM_PACKET_ERROR,
			{},
			{ FOMPacketError{ id, FOMPacketErrorCode::ERROR_UNHANDLED_PACKET_ID } }
		};
	}

	// Make sure to catch any deserialization errors so that
	// the library does not crash the consuming application.
	try {
		return serializer->DeserializePacket(bs);
	} catch (const std::exception& e) {
		return FOMPacket{
			ID_FOM_PACKET_ERROR,
			{},
			{ FOMPacketError{ id, FOMPacketErrorCode::ERROR_DESERIALIZATION } }
		};
	}
}

const IPacketSerializer* FOMPacketSerializer::GetSerializer(PacketIdentifier id) {
	auto it = serializerMap.find(id);
	if (it == serializerMap.end()) {
		return NULL;
	}
	return it->second;
}
