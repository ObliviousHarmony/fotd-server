#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/AccountType.h>
#include <fom-network/enums/WorldID.h>
#include <fom-network/types/Apartment.h>

namespace FOMNetwork {
namespace Packet {

enum LoginReturnStatus : uint8_t {
  LOGIN_RETURN_INVALID_LOGIN = 0,
  LOGIN_RETURN_SUCCESS = 1,
  LOGIN_RETURN_UNKNOWN_USERNAME = 2,
  LOGIN_RETURN_3 = 3,
  LOGIN_RETURN_INCORRECT_PASSWORD = 4,
  LOGIN_RETURN_CREATE_CHARACTER = 5,
  LOGIN_RETURN_CREATE_CHARACTER_ERROR = 6,
  LOGIN_RETURN_TEMP_BANNED = 7,
  LOGIN_RETURN_PERM_BANNED = 8,
  LOGIN_RETURN_DUPLICATE_IP = 9,
  LOGIN_RETURN_INTEGRITY_CHECK_FAILED = 10,
  LOGIN_RETURN_RUN_AS_ADMIN = 11,
  LOGIN_RETURN_ACCOUNT_LOCKED = 12,
  LOGIN_RETURN_NOT_PURCHASED = 13,
};

#pragma pack(push, 1)
struct LoginReturn {
  LoginReturnStatus status;
  uint32_t playerID;

  // ====== playerID != 0 ======
  Enum::AccountType accountType;
  uint8_t isVolunteer;
  uint8_t isNewPlayer;
  uint16_t clientVersion;

  uint8_t isBanned;
  uint8_t banLength[16]; // isBanned == 1
  uint8_t banReason[129]; // isBanned == 1

  uint8_t processBlacklistCount;
  uint32_t processBlacklist[128];

  uint8_t factionMOTD[1024];

  Type::Apartment defaultApartment;
  Enum::WorldID defaultApartmentWorldID;
  Enum::WorldID loginWorldID;
  // ===========================
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginReturn);

}  // namespace Packet
}  // namespace FOMNetwork
