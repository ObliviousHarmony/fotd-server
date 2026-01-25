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

    for (int i = 0; i < Enum::NUM_BASIC_EQUIPMENT_SLOTS; ++i)
      WriteBits(bs, data.equipmentSlots[i], 12);

    bool hasExtendedEquipment = false;
    for (int i = Enum::NUM_BASIC_EQUIPMENT_SLOTS; i < Enum::NUM_EQUIPMENT_SLOTS;
         ++i) {
      if (data.equipmentSlots[i] != 0) {
        hasExtendedEquipment = true;
        break;
      }
    }

    bs.Write(hasExtendedEquipment);
    if (hasExtendedEquipment) {
      for (int i = Enum::NUM_BASIC_EQUIPMENT_SLOTS;
           i < Enum::NUM_EQUIPMENT_SLOTS; ++i)
        WriteBits(bs, data.equipmentSlots[i], 12);
    }

    bs.Write(data.isCommander == 1);
    bs.Write(data.unknown2 == 1);
    bs.Write(data.unknown3 == 1);
    bs.Write(data.isGroupLeader == 1);
  }

  bool Read(RakNet::BitStream& bs, Type::Avatar& data) const {
    bool isFemale, isBlack;
    if (!bs.Read(isFemale)) return false;
    if (!bs.Read(isBlack)) return false;
    data.sex = isFemale ? Enum::FEMALE : Enum::MALE;
    data.race = isBlack ? Enum::BLACK : Enum::WHITE;
    if (!ReadBits(bs, data.face, 5)) return false;
    if (!ReadBits(bs, data.hair, 5)) return false;

    // This is a bug, factionID is 16-bits but that's what the client does.
    if (!ReadBits(bs, data.factionID, 32)) return false;

    if (!ReadBits(bs, data.rankID, 5)) return false;
    if (!ReadBits(bs, data.unknown1, 6)) return false;
    if (!ReadBits(bs, data.legacyFactionID, 4)) return false;

    for (int i = 0; i < Enum::NUM_BASIC_EQUIPMENT_SLOTS; ++i) {
      if (!ReadBits(bs, data.equipmentSlots[i], 12)) return false;
    }

    bool hasExtendedEquipment;
    if (!bs.Read(hasExtendedEquipment)) return false;
    if (hasExtendedEquipment) {
      for (int i = Enum::NUM_BASIC_EQUIPMENT_SLOTS;
           i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
        if (!ReadBits(bs, data.equipmentSlots[i], 12)) return false;
      }
    }

    bool isCommander, unknown2, unknown3, isGroupLeader;
    if (!bs.Read(isCommander)) return false;
    if (!bs.Read(unknown2)) return false;
    if (!bs.Read(unknown3)) return false;
    if (!bs.Read(isGroupLeader)) return false;
    data.isCommander = isCommander ? 1 : 0;
    data.unknown2 = unknown2 ? 1 : 0;
    data.unknown3 = unknown3 ? 1 : 0;
    data.isGroupLeader = isGroupLeader ? 1 : 0;

    return true;
  }
};

}  // namespace FOMNetwork
