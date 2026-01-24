#pragma once

#include <fom-network/types/Apartment.h>

#include "ItemListSerializer.h"
#include "TypeSerializer.h"

namespace FOMNetwork {

class ApartmentSerializer : protected TypeSerializer<Type::Apartment> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Apartment& data) const {
    ItemListSerializer itemListSerializer;

    bs.WriteCompressed(data.id);
    bs.WriteCompressed(data.type);
    bs.WriteCompressed(data.ownerPlayerID);
    bs.WriteCompressed(data.ownerFactionID);

    // Allowed Rank List
    bs.WriteCompressed((uint8_t)0);

    bs.Write(data.isOpen == 1);
    EncodeString(bs, data.ownerName);
    EncodeString(bs, data.entryCode);

    itemListSerializer.Write(bs, data.storageItems);

    bs.Write(data.isPublic == 1);
    bs.WriteCompressed(data.entryPrice);
    EncodeString(bs, data.publicName);
    EncodeString(bs, data.publicDescription);

    // Allowed Faction List
    bs.WriteCompressed((uint32_t)0);

    bs.Write(data.isDefault == 1);
    bs.Write(data.isFeatured == 1);
    bs.WriteCompressed(data.occupants);
  }

  bool Read(RakNet::BitStream& bs, Type::Apartment& data) const {
    ItemListSerializer itemListSerializer;
    uint8_t skipU8;
    uint32_t skipU32;

    if (!bs.ReadCompressed(data.id)) return false;
    if (!bs.ReadCompressed(data.type)) return false;
    if (!bs.ReadCompressed(data.ownerPlayerID)) return false;
    if (!bs.ReadCompressed(data.ownerFactionID)) return false;

    // Allowed Rank List
    bs.ReadCompressed(skipU8);

    data.isOpen = bs.ReadBit() ? 1 : 0;
    if (!DecodeString(bs, data.ownerName)) return false;
    if (!DecodeString(bs, data.entryCode)) return false;

    if (!itemListSerializer.Read(bs, data.storageItems)) return false;

    data.isPublic = bs.ReadBit() ? 1 : 0;
    if (!bs.ReadCompressed(data.entryPrice)) return false;
    if (!DecodeString(bs, data.publicName)) return false;
    if (!DecodeString(bs, data.publicDescription)) return false;

    // Allowed Faction List
    bs.ReadCompressed(skipU32);

    data.isDefault = bs.ReadBit() ? 1 : 0;
    data.isFeatured = bs.ReadBit() ? 1 : 0;
    if (!bs.ReadCompressed(data.occupants)) return false;

    return true;
  }
};

}  // namespace FOMNetwork
