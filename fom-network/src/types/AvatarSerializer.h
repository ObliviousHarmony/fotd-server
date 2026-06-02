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

    // This is a bug in the game client, factionId is 16-bits but this is what
    // the client does.
    WriteBits(bs, data.factionId, 32);

    WriteBits(bs, data.rankId, 5);
    WriteBits(bs, data.unknown1, 6);
    WriteBits(bs, data.legacyFactionId, 4);

    WriteBits(bs, data.shirt, 12);
    WriteBits(bs, data.bottoms, 12);
    WriteBits(bs, data.shoes, 12);

    bool hasAttachments = data.hat || data.head || data.eyes || data.shoulder ||
                          data.arms || data.torso || data.back || data.legs ||
                          data.hands;

    bs.Write(hasAttachments);
    if (hasAttachments) {
      WriteBits(bs, data.hat, 12);
      WriteBits(bs, data.head, 12);
      WriteBits(bs, data.eyes, 12);
      WriteBits(bs, data.shoulder, 12);
      WriteBits(bs, data.arms, 12);
      WriteBits(bs, data.torso, 12);
      WriteBits(bs, data.back, 12);
      WriteBits(bs, data.legs, 12);
      WriteBits(bs, data.hands, 12);
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

    // This is a bug in the game client, factionId is 16-bits but this is what
    // the client does.
    if (!ReadBits(bs, data.factionId, 32)) return false;

    if (!ReadBits(bs, data.rankId, 5)) return false;
    if (!ReadBits(bs, data.unknown1, 6)) return false;
    if (!ReadBits(bs, data.legacyFactionId, 4)) return false;

    if (!ReadBits(bs, data.shirt, 12)) return false;
    if (!ReadBits(bs, data.bottoms, 12)) return false;
    if (!ReadBits(bs, data.shoes, 12)) return false;

    bool hasAttachments;
    if (!bs.Read(hasAttachments)) return false;
    if (hasAttachments) {
      if (!ReadBits(bs, data.hat, 12)) return false;
      if (!ReadBits(bs, data.head, 12)) return false;
      if (!ReadBits(bs, data.eyes, 12)) return false;
      if (!ReadBits(bs, data.shoulder, 12)) return false;
      if (!ReadBits(bs, data.arms, 12)) return false;
      if (!ReadBits(bs, data.torso, 12)) return false;
      if (!ReadBits(bs, data.back, 12)) return false;
      if (!ReadBits(bs, data.legs, 12)) return false;
      if (!ReadBits(bs, data.hands, 12)) return false;
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
