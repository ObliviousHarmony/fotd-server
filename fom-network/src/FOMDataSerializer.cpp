#include <unordered_map>
#include <fom-network/FOMDataSerializer.h>

/**
 * We need to initialize the map with all of the serializers we want to be able to use.
 */
static std::unordered_map<uint32_t, IPacketSerializer*> serializerMap = {
	//{ ID_FOM_PACKET_START, &ExamplePacketSerializer::GetInstance() }
};

bool FOMDataSerializer::Serialize(RakNet::BitStream& bs, const PacketIdentifier id, const FOMData& data) {
	if (ShouldForwardRakNetPacket(id)) {
		return true;
	}

	const auto* serializer = GetSerializer(id);
	if (!serializer) {
		return false;
	}

	// Make sure to catch any serialization error so that the
	// library does not crash the consuming application.
	try {
		return serializer->SerializePacket(bs, data);
	} catch (const std::exception& e) {
		return false;
	}
}

FOMData FOMDataSerializer::Deserialize(RakNet::BitStream& bs, const PacketIdentifier id) {
	if (ShouldForwardRakNetPacket(id)) {
		return FOMData{};
	}

	const auto* serializer = GetSerializer(id);
	if (!serializer) {
		throw DeserializationError(
			FOMPacketError{ id, FOMPacketErrorCode::ERROR_UNHANDLED_PACKET_ID }
		);
	}

	// Make sure to catch any deserialization errors so that
	// the library does not crash the consuming application.
	try {
		return serializer->DeserializePacket(bs);
	} catch (const std::exception& e) {
		throw DeserializationError(
			FOMPacketError{ id, FOMPacketErrorCode::ERROR_DESERIALIZATION }
		);
	}
}

const IPacketSerializer* FOMDataSerializer::GetSerializer(PacketIdentifier id) {
	auto it = serializerMap.find(id);
	if (it == serializerMap.end()) {
		return NULL;
	}
	return it->second;
}
