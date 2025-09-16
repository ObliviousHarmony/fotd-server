#pragma once

#include <fom-network/PacketIdentifier.h>

// Include all packet types here.
#include <fom-network/packets/ReadPacketError.h>
#include <fom-network/packets/LoginRequest.h>
#include <fom-network/packets/LoginRequestReturn.h>

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

/**
 * A union representing all of FoM's packet data types.
 */
struct FOMData {
	union {
		FOMPacket::ReadPacketError readError;
		FOMPacket::LoginRequest loginRequest;
		FOMPacket::LoginRequestReturn loginRequestReturn;
	};
};

namespace FOMPacket {
	/**
	 * The network address for a system.
	 */
	struct NetworkAddress {
		uint32_t binaryAddress;
		uint16_t port;
	};
	ASSERT_BLITTABLE(NetworkAddress);

	/**
	 * A FoM network packet to be passed across the interop boundary.
	 */
	struct FOMPacket {
		PacketIdentifier ID;
		NetworkAddress sender;
		FOMData data;
	};
}

#pragma pack(pop)
