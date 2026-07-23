#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct CheckNameReturnPacket {
  uint32_t ownerPlayerId;
};
#pragma pack(pop)

ASSERT_BLITTABLE(CheckNameReturnPacket);

}  // namespace FOMNetwork
