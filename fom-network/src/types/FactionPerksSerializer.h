#pragma once

#include <fom-network/types/FactionPerks.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class FactionPerksSerializer : protected TypeSerializer<Type::FactionPerks> {
 public:
  void Write(RakNet::BitStream& bs, const Type::FactionPerks& data) const {
    bs.WriteCompressed(data.unknown1);
    bs.WriteCompressed(data.unknown2);
    bs.WriteCompressed(data.count);

    for (uint32_t i = 0; i < data.count; ++i) {
      bs.WriteCompressed(data.perks[i].id);
      bs.WriteCompressed(data.perks[i].level);
      bs.Write(data.perks[i].active == 1);
    }
  }

  bool Read(RakNet::BitStream& bs, Type::FactionPerks& data) const {
    if (!bs.ReadCompressed(data.unknown1)) return false;
    if (!bs.ReadCompressed(data.unknown2)) return false;
    if (!bs.ReadCompressed(data.count)) return false;

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
