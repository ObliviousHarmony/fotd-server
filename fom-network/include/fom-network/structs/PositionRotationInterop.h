#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/PositionInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct PositionRotationInterop {
  PositionInterop pos;
  uint16_t rot;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PositionRotationInterop);

}  // namespace FOMNetwork
