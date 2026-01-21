#pragma once

#include <fom-network/types/Avatar.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class AvatarSerializer : protected TypeSerializer<Type::Avatar> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Avatar& data) const {
    WriteBits(bs, data.sex, 1);
    WriteBits(bs, data.race, 1);
    WriteBits(bs, data.face, 5);
    WriteBits(bs, data.hair, 5);

    // This is a bug, factionID is 16-bits but that's what the client does.
    WriteBits(bs, data.factionID, 32);

    WriteBits(bs, data.rankID, 5);
    WriteBits(bs, 0, 6);
    WriteBits(bs, data.legacyFactionID, 4);

    WriteBits(bs, data.shirt, 12);
    WriteBits(bs, data.bottoms, 12);
    WriteBits(bs, data.shoes, 12);

    bool hasEquipment = false;
    for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
      if (data.equipmentSlots[i] != 0) {
        hasEquipment = true;
        break;
      }
    }

    if (hasEquipment) {
      bs.Write1();
      for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i)
        WriteBits(bs, data.equipmentSlots[i], 12);
    } else
      bs.Write0();

    bs.Write0();
    bs.Write0();
    bs.Write0();
    bs.Write0();
  }

  bool Read(RakNet::BitStream& bs, Type::Avatar& data) const {
    if (!ReadBits(bs, data.sex, 1)) return false;
    if (!ReadBits(bs, data.race, 1)) return false;
    if (!ReadBits(bs, data.face, 5)) return false;
    if (!ReadBits(bs, data.hair, 5)) return false;

    // This is a bug, factionID is 16-bits but that's what the client does.
    if (!ReadBits(bs, data.factionID, 32)) return false;

    if (!ReadBits(bs, data.rankID, 5)) return false;
    bs.IgnoreBits(6);
    if (!ReadBits(bs, data.legacyFactionID, 4)) return false;

    if (!ReadBits(bs, data.shirt, 12)) return false;
    if (!ReadBits(bs, data.bottoms, 12)) return false;
    if (!ReadBits(bs, data.shoes, 12)) return false;

    bool hasEquipment = bs.ReadBit();

    if (hasEquipment) {
      for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
        if (!ReadBits(bs, data.equipmentSlots[i], 12)) return false;
      }
    } else {
      for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i)
        data.equipmentSlots[i] = 0;
    }

    bs.IgnoreBits(1);
    bs.IgnoreBits(1);
    bs.IgnoreBits(1);
    bs.IgnoreBits(1);

    return true;
  }
};

}  // namespace FOMNetwork
