#pragma once

#include <raknet/BitStream.h>
#include <fom-network/FOMNetworkExport.h>
#include <fom-network/FOMPacket.h>

/**
 * Interface for packet serializers.
 */
class IPacketSerializer {
public:
	virtual ~IPacketSerializer() = default;

	// Always work with FOMPacket so all serializers can fit into one array
	virtual bool SerializePacket(RakNet::BitStream& bs, const FOMPacket& p) const = 0;
	virtual FOMPacket DeserializePacket(RakNet::BitStream& bs) const = 0;
};

/**
 * This macro makes it easy to define serializer classes without having to
 * write all of the same declaration boilerplate. It also ensures that we
 * can use the correct types for each packet.
 */
#define DECLARE_SERIALIZER(TYPE, PACKETID, UNION_FIELD)									\
class FOM_API TYPE##Serializer : public IPacketSerializer {								\
public:																					\
	static TYPE##Serializer& GetInstance() {											\
		static TYPE##Serializer instance;												\
		return instance;																\
	}																					\
	bool SerializePacket(RakNet::BitStream& bs, const FOMPacket& p) const override {	\
		return Serialize(bs, p.data.UNION_FIELD);										\
	}																					\
	FOMPacket DeserializePacket(RakNet::BitStream& bs) const override {					\
		TYPE obj = Deserialize(bs);														\
		FOMPacket packet{};																\
		packet.ID = PacketIdentifier::PACKETID;											\
		packet.data.UNION_FIELD = obj;													\
		return packet;																	\
	}																					\
	bool Serialize(RakNet::BitStream& bs, const TYPE& p) const;							\
	TYPE Deserialize(RakNet::BitStream& bs) const;										\
};																			

/**
 * Declare all of the serializers. Keep in mind that they must be:
 * <PacketTypeName>Serializer
 */
DECLARE_SERIALIZER(ExamplePacket, ID_FOM_PACKET_START, example)
