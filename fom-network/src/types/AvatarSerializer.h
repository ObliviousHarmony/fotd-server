#pragma once

#include <fom-network/types/Avatar.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class AvatarSerializer : protected TypeSerializer<Type::Avatar> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Avatar& model) const {
    WriteBits(bs, model.sex, 1);
    WriteBits(bs, model.race, 1);
    WriteBits(bs, model.face, 5);
    WriteBits(bs, model.hair, 5);

    // This is a bug, factionID is 16-bits but that's what the client does.
    WriteBits(bs, model.factionID, 32);

    WriteBits(bs, model.rankID, 5);
    WriteBits(bs, 0, 6);
    WriteBits(bs, model.legacyFactionID, 4);

    WriteBits(bs, model.shirt, 12);
    WriteBits(bs, model.bottoms, 12);
    WriteBits(bs, model.shoes, 12);

    bool hasEquipment = false;
    for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
      if (model.equipmentSlots[i] != 0) {
        hasEquipment = true;
        break;
      }
    }

    if (hasEquipment) {
      bs.Write1();
      for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i)
        WriteBits(bs, model.equipmentSlots[i], 12);
    } else
      bs.Write0();

    bs.Write0();
    bs.Write0();
    bs.Write0();
    bs.Write0();
  }

  bool Read(RakNet::BitStream& bs, Type::Avatar& model) const {
    if (!ReadBits(bs, model.sex, 1)) return false;
    if (!ReadBits(bs, model.race, 1)) return false;
    if (!ReadBits(bs, model.face, 5)) return false;
    if (!ReadBits(bs, model.hair, 5)) return false;

    // This is a bug, factionID is 16-bits but that's what the client does.
    if (!ReadBits(bs, model.factionID, 32)) return false;

    if (!ReadBits(bs, model.rankID, 5)) return false;
    bs.IgnoreBits(6);
    if (!ReadBits(bs, model.legacyFactionID, 4)) return false;

    if (!ReadBits(bs, model.shirt, 12)) return false;
    if (!ReadBits(bs, model.bottoms, 12)) return false;
    if (!ReadBits(bs, model.shoes, 12)) return false;

    bool hasEquipment = bs.ReadBit();

    if (hasEquipment) {
      for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
        if (!ReadBits(bs, model.equipmentSlots[i], 12)) return false;
      }
    } else {
      for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i)
        model.equipmentSlots[i] = 0;
    }

    bs.IgnoreBits(1);
    bs.IgnoreBits(1);
    bs.IgnoreBits(1);
    bs.IgnoreBits(1);

    return true;
  }
};

}  // namespace FOMNetwork
