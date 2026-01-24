#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/EquipmentSlot.h>
#include <fom-network/enums/WeaponSlot.h>
#include <fom-network/types/Item.h>
#include <fom-network/types/ItemList.h>

namespace FOMNetwork {
namespace Type {

constexpr int NUM_UNKNOWN_ITEM_SLOTS = 6;

#pragma pack(push, 1)
struct Inventory {
  ItemList inventory;
  Item equipment[Enum::NUM_EQUIPMENT_SLOTS];
  Item weapons[Enum::NUM_WEAPON_SLOTS];
  Item unknown1[NUM_UNKNOWN_ITEM_SLOTS];
  ItemList storage;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Inventory);

}  // namespace Type
}  // namespace FOMNetwork
