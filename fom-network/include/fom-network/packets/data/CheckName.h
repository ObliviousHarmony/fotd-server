#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct CheckName {
  uint8_t name[20];
};
#pragma pack(pop)

ASSERT_BLITTABLE(CheckName);

}  // namespace Packet
}  // namespace FOMNetwork
