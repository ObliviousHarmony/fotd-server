#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct NetworkAddress {
  uint32_t binaryAddress;
  uint16_t port;
};
#pragma pack(pop)

ASSERT_BLITTABLE(NetworkAddress);

}  // namespace FOMNetwork
