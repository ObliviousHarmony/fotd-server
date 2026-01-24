#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/EquipmentSlot.h>
#include <fom-network/enums/WeaponSlot.h>
#include <fom-network/types/Item.h>
#include <fom-network/types/ItemList.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct Inventory {
  ItemList inventory;
  Item equipment[Enum::NUM_EQUIPMENT_SLOTS];
  Item weapons[Enum::NUM_WEAPON_SLOTS];
  ItemList storage;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Inventory);

}  // namespace Type
}  // namespace FOMNetwork
