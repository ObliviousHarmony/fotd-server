#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

enum LoginRequestReturnStatus : uint8_t {
  LOGIN_REQUEST_RETURN_INVALID_INFORMATION = 0,
  LOGIN_REQUEST_RETURN_SUCCESS = 1,
  LOGIN_REQUEST_RETURN_VERSION_MISMATCH = 2,
  LOGIN_REQUEST_RETURN_ALREADY_LOGGED_IN = 3,
};

#pragma pack(push, 1)
struct LoginRequestReturn {
  LoginRequestReturnStatus status;
  uint8_t username[32];
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginRequestReturn);

}  // namespace Packet
}  // namespace FOMNetwork
