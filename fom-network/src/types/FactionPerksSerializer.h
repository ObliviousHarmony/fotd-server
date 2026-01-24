#pragma once

#include <fom-network/types/FactionPerks.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class FactionPerksSerializer : protected TypeSerializer<Type::FactionPerks> {
 public:
  void Write(RakNet::BitStream& bs, const Type::FactionPerks& data) const {
    bs.WriteCompressed(data.field_0x0);
    bs.WriteCompressed(data.field_0x4);
    bs.WriteCompressed(data.count);

    for (uint32_t i = 0; i < data.count; ++i) {
      bs.WriteCompressed(data.perks[i].id);
      bs.WriteCompressed(data.perks[i].level);
      if (data.perks[i].active)
        bs.Write1();
      else
        bs.Write0();
    }
  }

  bool Read(RakNet::BitStream& bs, Type::FactionPerks& data) const {
    if (!bs.ReadCompressed(data.field_0x0)) return false;
    if (!bs.ReadCompressed(data.field_0x4)) return false;
    if (!bs.ReadCompressed(data.count)) return false;

    for (uint32_t i = 0; i < data.count; ++i) {
      if (!bs.ReadCompressed(data.perks[i].id)) return false;
      if (!bs.ReadCompressed(data.perks[i].level)) return false;
      data.perks[i].active = bs.ReadBit() ? 1 : 0;
    }

    return true;
  }
};

}  // namespace FOMNetwork
