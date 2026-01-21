#pragma once

#include <fom-network/types/Apartment.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class ApartmentSerializer
    : protected TypeSerializer<Type::Apartment> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Apartment& data) const {
    bs.WriteCompressed(data.id);
    WriteBits(bs, data.type, 8);
    bs.WriteCompressed(data.ownerPlayerID);
    bs.WriteCompressed(data.ownerFactionID);

    // Allowed Rank List
    bs.Write0();

    bs.Write(data.isOpen == 1);
    EncodeString(bs, data.ownerName);
    EncodeString(bs, data.entryCode);

    // Storage Item List
    bs.Write0();

    bs.Write(data.isPublic == 1);
    bs.WriteCompressed(data.entryPrice);
    EncodeString(bs, data.publicName);
    EncodeString(bs, data.publicDescription);

    // Allowed Faction List
    bs.Write0();

    bs.Write(data.isDefault == 1);
    bs.Write(data.isFeatured == 1);
    bs.WriteCompressed(data.occupants);
  }

  bool Read(RakNet::BitStream& bs, Type::Apartment& data) const {
    if (!bs.ReadCompressed(data.id)) return false;
    if (!ReadBits(bs, data.type, 8)) return false;
    if (!bs.ReadCompressed(data.ownerPlayerID)) return false;
    if (!bs.ReadCompressed(data.ownerFactionID)) return false;

    // Allowed Rank List
    if (bs.ReadBit()) return false;

    data.isOpen = bs.ReadBit() ? 1 : 0;
    if (!DecodeString(bs, data.ownerName)) return false;
    if (!DecodeString(bs, data.entryCode)) return false;

    // Storage Item List
    if (bs.ReadBit()) return false;

    data.isPublic = bs.ReadBit() ? 1 : 0;
    if (!bs.ReadCompressed(data.entryPrice)) return false;
    if (!DecodeString(bs, data.publicName)) return false;
    if (!DecodeString(bs, data.publicDescription)) return false;

    // Allowed Faction List
    if (bs.ReadBit()) return false;

    data.isDefault = bs.ReadBit() ? 1 : 0;
    data.isFeatured = bs.ReadBit() ? 1 : 0;
    if (!bs.ReadCompressed(data.occupants)) return false;

    return true;
  }
};

}  // namespace FOMNetwork
