#pragma once

#include <fom-network/PacketIdentifier.h>

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

struct LoginRequest {
	char username[19];
	uint16_t clientVersion;
};
ASSERT_BLITTABLE(LoginRequest);

#pragma pack(pop)
