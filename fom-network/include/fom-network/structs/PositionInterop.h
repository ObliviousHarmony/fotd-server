#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct PositionInterop {
  int16_t x;
  int16_t y;
  int16_t z;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PositionInterop);

}  // namespace FOMNetwork
