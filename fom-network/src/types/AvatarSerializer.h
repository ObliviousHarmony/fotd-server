#pragma once

#include <fom-network/types/Avatar.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class AvatarSerializer : protected TypeSerializer<Type::Avatar> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Avatar& data) const {
    bs.Write(data.sex == 1);
    bs.Write(data.race == 1);
    WriteBits(bs, data.face, 5);
    WriteBits(bs, data.hair, 5);

    // This is a bug, factionID is 16-bits but that's what the client does.
    WriteBits(bs, data.factionID, 32);

    WriteBits(bs, data.rankID, 5);
    WriteBits(bs, data.unknown1, 6);
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

    bs.Write(data.isCommander == 1);
    bs.Write(data.unknown2 == 1);
    bs.Write(data.unknown3 == 1);
    bs.Write(data.isGroupLeader == 1);
  }

  bool Read(RakNet::BitStream& bs, Type::Avatar& data) const {
    data.sex = bs.ReadBit() ? 1 : 0;
    data.race = bs.ReadBit() ? 1 : 0;
    if (!ReadBits(bs, data.face, 5)) return false;
    if (!ReadBits(bs, data.hair, 5)) return false;

    // This is a bug, factionID is 16-bits but that's what the client does.
    if (!ReadBits(bs, data.factionID, 32)) return false;

    if (!ReadBits(bs, data.rankID, 5)) return false;
    if (!ReadBits(bs, data.unknown1, 6)) return false;
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

    data.isCommander = bs.ReadBit() ? 1 : 0;
    data.unknown2 = bs.ReadBit() ? 1 : 0;
    data.unknown3 = bs.ReadBit() ? 1 : 0;
    data.isGroupLeader = bs.ReadBit() ? 1 : 0;

    return true;
  }
};

}  // namespace FOMNetwork
