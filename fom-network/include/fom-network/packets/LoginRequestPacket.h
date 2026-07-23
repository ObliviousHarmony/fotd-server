#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct LoginRequestPacket {
  uint8_t username[BufferSizes::USERNAME];
  uint16_t clientVersion;
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginRequestPacket);

}  // namespace FOMNetwork
