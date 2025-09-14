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
	virtual bool SerializePacket(RakNet::BitStream& bs, const FOMData& data) const = 0;
	virtual FOMData DeserializePacket(RakNet::BitStream& bs) const = 0;
};

/**
 * This macro makes it easy to define serializer classes without having to
 * write all of the same declaration boilerplate. It also ensures that we
 * can use the correct types for each packet.
 */
#define DECLARE_SERIALIZER(TYPE, UNION_FIELD)											\
class FOM_API TYPE##Serializer : public IPacketSerializer {								\
public:																					\
	static TYPE##Serializer& GetInstance() {											\
		static TYPE##Serializer instance;												\
		return instance;																\
	}																					\
	bool SerializePacket(RakNet::BitStream& bs, const FOMData& data) const override {	\
		return Serialize(bs, data.UNION_FIELD);											\
	}																					\
	FOMData DeserializePacket(RakNet::BitStream& bs) const override {					\
		FOMData data{};														\
		data.UNION_FIELD = Deserialize(bs);												\
		return data;																	\
	}																					\
	bool Serialize(RakNet::BitStream& bs, const TYPE& data) const;						\
	TYPE Deserialize(RakNet::BitStream& bs) const;										\
};

/**
 * Declare all of the serializers. Keep in mind that they must be:
 * <PacketTypeName>Serializer
 */
DECLARE_SERIALIZER(FOMPacketError, error)
