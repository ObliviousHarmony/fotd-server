#pragma once

#include <fom-network/PacketIdentifier.h>

/**
 * Make sure that we pack the structs the same way that C# does.
 */
#pragma pack(push, 1)

namespace FOMPacket {
/**
 * Status codes for login returns.
 */
enum LoginReturnStatus : uint8_t {
  LOGIN_RETURN_INVALID_LOGIN,
  LOGIN_RETURN_SUCCESS,
  LOGIN_RETURN_INVALID_USERNAME,
  LOGIN_RETURN_X1, // Unknown
  LOGIN_RETURN_INVALID_PASSWORD,
  LOGIN_RETURN_CREATE_CHARACTER,
  LOGIN_RETURN_CREATE_CHARACTER_ERROR,
  LOGIN_RETURN_TEMP_BANNED,
  LOGIN_RETURN_PERM_BANNED,
  LOGIN_RETURN_DUPLICATE_ACCOUNTS,
  LOGIN_RETURN_INTEGRITY_CHECK_FAILED,
  LOGIN_RETURN_CLIENT_ERROR,
  LOGIN_RETURN_LOCKED
};

struct LoginReturn {
  LoginReturnStatus status;
  uint32_t playerID;
  uint8_t accountType;
  uint8_t isVolunteer;
  uint16_t ClientVersion;
  uint8_t isBanned;
};
ASSERT_BLITTABLE(LoginReturn);
}  // namespace FOMPacket

#pragma pack(pop)
