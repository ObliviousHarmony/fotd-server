#pragma once

#include <memory>
#include <unordered_map>
#include <fom-network/FOMNetworkExport.h>
#include <fom-network/FOMPacket.h>
#include <fom-network/PacketSerializers.h>

/**
 * Handles packet serialization and deserialization based on packet ID.
 */
class FOM_API FOMPacketSerializer {
public:
	/**
	* Serializes a packet structure into a bitstream buffer.
	*
	* @param packet The packet to serialize.
	*/
	static bool Serialize(RakNet::BitStream& bs, const FOMPacket& packet);

	/**
	* Deserializes a bitstream buffer into a packet structure.
	*
	* @param bitstream The bitstream to deserialize.
	*/
	static FOMPacket Deserialize(RakNet::BitStream& bitstream);

private:
    FOMPacketSerializer();
	~FOMPacketSerializer();

	/**
	* Fetches the serializer for a given packet ID.
	*
	* @param id The packet ID to fetch the serializer for.
	*/
	static const IPacketSerializer* GetSerializer(PacketIdentifier id);
};
