#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

// TEMPORARY capture probe: see WeaponFire.h. Grabs ID_RELOAD's raw payload bits
// so a live reload test can reveal the format.
#pragma pack(push, 1)
struct Reload {
  uint16_t bitCount;
  uint8_t data[128];
};
#pragma pack(pop)

ASSERT_BLITTABLE(Reload);

}  // namespace Packet
}  // namespace FOMNetwork
