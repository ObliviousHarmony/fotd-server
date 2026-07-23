#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct FactionPerkInterop {
  uint16_t id;
  uint8_t level;
  uint8_t active;
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionPerkInterop);

}  // namespace FOMNetwork
