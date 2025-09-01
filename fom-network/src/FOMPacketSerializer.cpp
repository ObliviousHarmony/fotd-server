#include <unordered_map>
#include <fom-network/FOMPacketSerializer.h>

/**
 * We need to initialize the map with all of the serializers we want to be able to use.
 */
static std::unordered_map<uint32_t, IPacketSerializer*> serializerMap = {
	{ ID_USER_PACKET_ENUM, &ExamplePacketSerializer::GetInstance() }
};

bool FOMPacketSerializer::Serialize(RakNet::BitStream& bs, const FOMPacket& p) {
    const auto* serializer = GetSerializer(p.ID);
    if (!serializer) {
		return false;
    }

    // The first byte in the BitStream will always be the packet ID.
    bs.Write(p.ID);

    return serializer->SerializePacket(bs, p);
}

FOMPacket FOMPacketSerializer::Deserialize(RakNet::BitStream& bs) {
    // The first byte in the BitStream will always be the packet ID.
    PacketIdentifier id;
    if (!bs.Read(id)) {
        return INVALID_PACKET;
    }

    const auto* serializer = GetSerializer(id);
    if (!serializer) {
        return INVALID_PACKET;
    }

    return serializer->DeserializePacket(bs);
}

const IPacketSerializer* FOMPacketSerializer::GetSerializer(PacketIdentifier id) {
    auto it = serializerMap.find(id);
    if (it == serializerMap.end()) {
        return NULL;
    }
    return it->second;
}
