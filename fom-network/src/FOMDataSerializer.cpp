#include <unordered_map>
#include <fom-network/FOMDataSerializer.h>

/**
 * We need to initialize the map with all of the serializers we want to be able to use.
 */
static std::unordered_map<uint32_t, IWriter*> writerMap = {
	//{ ID_FOM_PACKET_START, &ExamplePacketSerializer::GetInstance() }
};
static std::unordered_map<uint32_t, IReader*> readerMap = {
	{ ID_LOGIN_REQUEST, &LoginRequestPacketSerializer::GetInstance() }
};

bool FOMDataSerializer::Write(RakNet::BitStream& bs, const PacketIdentifier id, const FOMData& data) {
	if (ShouldForwardRakNetPacket(id)) {
		return true;
	}

	const auto* writer = GetWriter(id);
	if (!writer) {
		return false;
	}

	// Make sure to catch any serialization error so that the
	// library does not crash the consuming application.
	try {
		return writer->Write(bs, data);
	} catch (const std::exception& e) {
		return false;
	}
}

FOMData FOMDataSerializer::Read(RakNet::BitStream& bs, const PacketIdentifier id) {
	if (ShouldForwardRakNetPacket(id)) {
		return FOMData{};
	}

	const auto* reader = GetReader(id);
	if (!reader) {
		throw ReadError(
			FOMPacketError{ id, FOMPacketErrorCode::ERROR_UNHANDLED_PACKET_ID }
		);
	}

	// Make sure to catch any deserialization errors so that
	// the library does not crash the consuming application.
	try {
		return reader->Read(bs);
	} catch (const std::exception& e) {
		throw ReadError(
			FOMPacketError{ id, FOMPacketErrorCode::ERROR_READ }
		);
	}
}

const IWriter* FOMDataSerializer::GetWriter(PacketIdentifier id) {
	auto it = writerMap.find(id);
	if (it == writerMap.end()) {
		return NULL;
	}
	return it->second;
}

const IReader* FOMDataSerializer::GetReader(PacketIdentifier id) {
	auto it = readerMap.find(id);
	if (it == readerMap.end()) {
		return NULL;
	}
	return it->second;
}
