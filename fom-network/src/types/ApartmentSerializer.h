#pragma once

#include <fom-network/types/Apartment.h>

#include "ItemListSerializer.h"
#include "TypeSerializer.h"

namespace FOMNetwork {
namespace Type {

class ApartmentSerializer : protected TypeSerializer<Type::Apartment> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Apartment& data) const {
    ItemListSerializer itemListSerializer;

    bs.WriteCompressed(data.id);
    bs.WriteCompressed(data.type);
    bs.WriteCompressed(data.ownerPlayerId);
    bs.WriteCompressed(data.ownerFactionId);

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
    if (!bs.ReadCompressed(data.ownerPlayerId)) return false;
    if (!bs.ReadCompressed(data.ownerFactionId)) return false;

    // Allowed Rank List
    bs.ReadCompressed(skipU8);

    bool isOpen;
    if (!bs.Read(isOpen)) return false;
    data.isOpen = isOpen ? 1 : 0;
    if (!DecodeString(bs, data.ownerName)) return false;
    if (!DecodeString(bs, data.entryCode)) return false;

    if (!itemListSerializer.Read(bs, data.storageItems)) return false;

    bool isPublic;
    if (!bs.Read(isPublic)) return false;
    data.isPublic = isPublic ? 1 : 0;
    if (!bs.ReadCompressed(data.entryPrice)) return false;
    if (!DecodeString(bs, data.publicName)) return false;
    if (!DecodeString(bs, data.publicDescription)) return false;

    // Allowed Faction List
    bs.ReadCompressed(skipU32);

    bool isDefault, isFeatured;
    if (!bs.Read(isDefault)) return false;
    if (!bs.Read(isFeatured)) return false;
    data.isDefault = isDefault ? 1 : 0;
    data.isFeatured = isFeatured ? 1 : 0;
    if (!bs.ReadCompressed(data.occupants)) return false;

    return true;
  }
};

}  // namespace Type
}  // namespace FOMNetwork
