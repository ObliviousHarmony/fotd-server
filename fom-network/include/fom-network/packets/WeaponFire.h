#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

// TEMPORARY capture probe: ID_WEAPONFIRE's real wire format is not yet known, so
// this reader just grabs the raw payload bits so a live fire test can reveal them.
// Replace with the real struct once decoded (see "Weapons and Ammo" in the vault).
#pragma pack(push, 1)
struct WeaponFire {
  uint16_t bitCount;
  uint8_t data[128];
};
#pragma pack(pop)

ASSERT_BLITTABLE(WeaponFire);

}  // namespace Packet
}  // namespace FOMNetwork
