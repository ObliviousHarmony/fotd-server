#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/FactionPerk.h>

namespace FOMNetwork {
namespace Type {

constexpr int MAX_FACTION_PERKS = 32;

#pragma pack(push, 1)
struct FactionPerks {
  uint32_t unknown1;
  uint32_t unknown2;
  uint32_t count;
  FactionPerk perks[MAX_FACTION_PERKS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionPerks);

}  // namespace Type
}  // namespace FOMNetwork
