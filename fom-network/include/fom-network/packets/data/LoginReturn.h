#pragma once

#include <fom-network/Common.h>
#include <fom-network/packets/models/WorldOverviewModel.h>

namespace FOMNetwork {
namespace Packet {

enum LoginReturnStatus : uint8_t {
  LOGIN_RETURN_INVALID_LOGIN = 0,
  LOGIN_RETURN_SUCCESS = 1,
  LOGIN_RETURN_INVALID_USERNAME = 2,
  LOGIN_RETURN_X1 = 3,  // Unknown
  LOGIN_RETURN_INVALID_PASSWORD = 4,
  LOGIN_RETURN_CREATE_CHARACTER = 5,
  LOGIN_RETURN_CREATE_CHARACTER_ERROR = 6,
  LOGIN_RETURN_TEMP_BANNED = 7,
  LOGIN_RETURN_PERM_BANNED = 8,
  LOGIN_RETURN_DUPLICATE_ACCOUNTS = 9,
  LOGIN_RETURN_INTEGRITY_CHECK_FAILED = 10,
  LOGIN_RETURN_CLIENT_ERROR = 11,
  LOGIN_RETURN_LOCKED = 12
};

#pragma pack(push, 1)
struct LoginReturn {
  LoginReturnStatus status;
  PlayerID_t playerID;
  uint8_t accountType;
  uint8_t isVolunteer;
  uint16_t clientVersion;
  WorldOverviewModel worldOverview;
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginReturn);

}  // namespace Packet
}  // namespace FOMNetwork
