#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

enum LoginRequestReturnStatus : uint8_t {
  LOGIN_REQUEST_INVALID_INFORMATION,
  LOGIN_REQUEST_SUCCESS,
  LOGIN_REQUEST_OUTDATED_CLIENT,
  LOGIN_REQUEST_ALREADY_LOGGED_IN
};

#pragma pack(push, 1)
struct LoginRequestReturn {
  LoginRequestReturnStatus status;
  uint8_t username[19];
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginRequestReturn);

}  // namespace Packet
}  // namespace FOMNetwork
