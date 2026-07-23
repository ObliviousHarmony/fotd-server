#pragma once

#include <fom-network/structs/faction/FactionPerksInterop.h>

#include "../InteropTypeSerializer.h"

namespace FOMNetwork {

class FactionPerksInteropSerializer
    : protected InteropTypeSerializer<FactionPerksInterop> {
 public:
  void Write(RakNet::BitStream& bs, const FactionPerksInterop& data) const {
    uint32_t count = data.count;
    if (count > MAX_FACTION_PERKS) count = MAX_FACTION_PERKS;

    bs.WriteCompressed(data.unknown1);
    bs.WriteCompressed(data.unknown2);
    bs.WriteCompressed(count);

    for (uint32_t i = 0; i < count; ++i) {
      bs.WriteCompressed(data.perks[i].id);
      bs.WriteCompressed(data.perks[i].level);
      bs.Write(data.perks[i].active == 1);
    }
  }

  bool Read(RakNet::BitStream& bs, FactionPerksInterop& data) const {
    if (!bs.ReadCompressed(data.unknown1)) return false;
    if (!bs.ReadCompressed(data.unknown2)) return false;
    if (!bs.ReadCompressed(data.count)) return false;
    if (data.count > MAX_FACTION_PERKS) return false;

    for (uint32_t i = 0; i < data.count; ++i) {
      if (!bs.ReadCompressed(data.perks[i].id)) return false;
      if (!bs.ReadCompressed(data.perks[i].level)) return false;
      bool active;
      if (!bs.Read(active)) return false;
      data.perks[i].active = active ? 1 : 0;
    }

    return true;
  }
};

}  // namespace FOMNetwork
