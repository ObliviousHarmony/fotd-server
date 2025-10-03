#pragma once

#include <fom-network/Common.h>
#include <fom-network/packets/models/ApartmentModel.h>
#include <fom-network/packets/models/WorldOverviewModel.h>

namespace FOMNetwork {
namespace Packet {

enum LoginReturnStatus : uint8_t {
  LOGIN_RETURN_INVALID_LOGIN,
  LOGIN_RETURN_SUCCESS,
  LOGIN_RETURN_INVALID_USERNAME,
  LOGIN_RETURN_X1,  // Unknown
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

#pragma pack(push, 1)
struct LoginReturn {
  LoginReturnStatus status;
  uint32_t playerID;
  uint8_t accountType;
  uint8_t isVolunteer;
  uint16_t clientVersion;
  WorldOverviewModel worldOverview;
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginReturn);

}  // namespace Packet
}  // namespace FOMNetwork
