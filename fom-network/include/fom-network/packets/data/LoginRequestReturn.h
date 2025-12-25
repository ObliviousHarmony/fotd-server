#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

enum LoginRequestReturnStatus : uint8_t {
  LOGIN_REQUEST_RETURN_OK = 0,
  LOGIN_REQUEST_RETURN_VERSION_MISMATCH = 1,
  LOGIN_REQUEST_RETURN_BANNED = 2,
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
