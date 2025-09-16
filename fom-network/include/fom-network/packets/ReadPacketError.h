#pragma once

#include <fom-network/PacketIdentifier.h>

/**
 * Error codes for sending/receiving packets.
 */
enum ReadPacketErrorCode : uint8_t {
	ERROR_MISSING_PACKET_ID,
	ERROR_UNHANDLED_PACKET_ID,
	ERROR_READ
};

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

/**
 * An error took place when processing/sending a packet.
 */
struct ReadPacketError {
	PacketIdentifier offendingID;
	ReadPacketErrorCode errorCode;
};
ASSERT_BLITTABLE(ReadPacketError);

#pragma pack(pop)
