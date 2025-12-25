#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct LoginRequest {
  uint8_t username[32];
  uint16_t clientVersion;
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginRequest);

}  // namespace Packet
}  // namespace FOMNetwork
