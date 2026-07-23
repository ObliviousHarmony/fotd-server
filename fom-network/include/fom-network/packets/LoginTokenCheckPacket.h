#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct LoginTokenCheckPacket {
  uint8_t fromServer;

  uint8_t requestToken[32];  // fromServer == 0

  uint8_t success;                          // fromServer == 1
  uint8_t username[BufferSizes::USERNAME];  // fromServer == 1
};
#pragma pack(pop)

ASSERT_BLITTABLE(LoginTokenCheckPacket);

}  // namespace FOMNetwork
