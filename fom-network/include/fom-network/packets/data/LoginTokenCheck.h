#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct LoginTokenCheck {
  uint8_t fromServer;

  uint8_t requestToken[32];  // fromServer == 0

  uint8_t success;       // fromServer == 1
  uint8_t username[32];  // fromServer == 1
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginTokenCheck);

}  // namespace Packet
}  // namespace FOMNetwork
