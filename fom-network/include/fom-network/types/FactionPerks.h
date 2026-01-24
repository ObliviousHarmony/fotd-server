#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/FactionPerk.h>

namespace FOMNetwork {
namespace Type {

constexpr int MAX_FACTION_PERKS = 32;

// Wire format:
// field_0x0 (uint32_t), field_0x4 (unknown call), count (uint32_t),
// then array of FactionPerk (id: u16, level: u8, active: 1 bit)
#pragma pack(push, 1)
struct FactionPerks {
  uint32_t field_0x0;
  uint32_t field_0x4;
  uint32_t count;
  FactionPerk perks[MAX_FACTION_PERKS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionPerks);

}  // namespace Type
}  // namespace FOMNetwork
