#pragma once

#include <fom-network/PacketIdentifier.h>

/**
 * Status codes for login requests.
 */
enum LoginRequestStatus : uint8_t {
	LOGIN_REQUEST_INVALID_INFORMATION,
	LOGIN_REQUEST_SUCCESS,
	LOGIN_REQUEST_OUTDATED_CLIENT,
	LOGIN_REQUEST_ALREADY_LOGGED_IN
};

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

struct LoginRequestReturnPacket {
	LoginRequestStatus status;
	char username[19];
};
ASSERT_BLITTABLE(LoginRequestReturnPacket);

#pragma pack(pop)
