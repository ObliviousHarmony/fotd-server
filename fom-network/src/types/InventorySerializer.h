#pragma once

#include <fom-network/types/Inventory.h>

#include "ItemListSerializer.h"
#include "ItemSerializer.h"
#include "TypeSerializer.h"

namespace FOMNetwork {

class InventorySerializer : protected TypeSerializer<Type::Inventory> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Inventory& data) const {
    ItemListSerializer itemListSerializer;
    ItemSerializer itemSerializer;

    itemListSerializer.Write(bs, data.inventory);
    for (uint8_t i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
      if (data.equipment[i].id != 0) {
        bs.Write1();
        itemSerializer.Write(bs, data.equipment[i]);
      } else {
        bs.Write0();
      }
    }

    for (uint8_t i = 0; i < Enum::NUM_WEAPON_SLOTS; ++i) {
      if (data.weapons[i].id != 0) {
        bs.Write1();
        itemSerializer.Write(bs, data.weapons[i]);
      } else {
        bs.Write0();
      }
    }

    for (uint8_t i = 0; i < Type::NUM_UNKNOWN_ITEM_SLOTS; ++i) {
      if (data.unknown1[i].id != 0) {
        bs.Write1();
        itemSerializer.Write(bs, data.unknown1[i]);
      } else {
        bs.Write0();
      }
    }

    itemListSerializer.Write(bs, data.storage);
  }

  bool Read(RakNet::BitStream& bs, Type::Inventory& data) const {
    ItemListSerializer itemListSerializer;
    ItemSerializer itemSerializer;

    if (!itemListSerializer.Read(bs, data.inventory)) return false;

    for (uint8_t i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
      if (bs.ReadBit()) {
        if (!itemSerializer.Read(bs, data.equipment[i])) return false;
      } else {
        data.equipment[i] = {};
      }
    }

    for (uint8_t i = 0; i < Enum::NUM_WEAPON_SLOTS; ++i) {
      if (bs.ReadBit()) {
        if (!itemSerializer.Read(bs, data.weapons[i])) return false;
      } else {
        data.weapons[i] = {};
      }
    }

    for (uint8_t i = 0; i < Type::NUM_UNKNOWN_ITEM_SLOTS; ++i) {
      if (bs.ReadBit()) {
        if (!itemSerializer.Read(bs, data.unknown1[i])) return false;
      } else {
        data.unknown1[i] = {};
      }
    }

    if (!itemListSerializer.Read(bs, data.storage)) return false;

    return true;
  }
};

}  // namespace FOMNetwork
