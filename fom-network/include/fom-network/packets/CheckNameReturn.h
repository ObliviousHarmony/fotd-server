#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct CheckNameReturn {
  uint32_t existingAccountID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(CheckNameReturn);

}  // namespace Packet
}  // namespace FOMNetwork
