#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/faction/FactionPerkInterop.h>

namespace FOMNetwork {

constexpr int MAX_FACTION_PERKS = 32;

#pragma pack(push, 1)
struct FactionPerksInterop {
  uint32_t unknown1;
  uint32_t unknown2;
  uint32_t count;
  FactionPerkInterop perks[MAX_FACTION_PERKS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(FactionPerksInterop);

}  // namespace FOMNetwork
