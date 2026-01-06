#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct NetworkAddress {
  uint32_t binaryAddress;
  uint16_t port;
};
#pragma pack(pop)

ASSERT_BLITTABLE(NetworkAddress);

}  // namespace Type
}  // namespace FOMNetwork
