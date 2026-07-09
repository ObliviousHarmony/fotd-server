#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum ItemSlot : uint8_t {
  ITEM_SLOT_NONE = 0,

  ITEM_SLOT_WEAPON_1 = 1,
  ITEM_SLOT_WEAPON_1 = 2,
  ITEM_SLOT_WEAPON_1 = 3,

  ITEM_SLOT_HEAD = 5,
  ITEM_SLOT_EYES = 6,
  ITEM_SLOT_SHOULDERS = 7,
  ITEM_SLOT_TORSO = 8,
  ITEM_SLOT_ARMS = 9,
  ITEM_SLOT_HANDS = 10,
  ITEM_SLOT_LEGS = 11,
  ITEM_SLOT_BACK = 12,
  ITEM_SLOT_HAT = 13,
  ITEM_SLOT_SHIRT = 14,
  ITEM_SLOT_PANTS = 15,
  ITEM_SLOT_SHOES = 16,

  ITEM_SLOT_QUICKSLOT_1 = 18,
  ITEM_SLOT_QUICKSLOT_2 = 19,
  ITEM_SLOT_QUICKSLOT_3 = 20,
  ITEM_SLOT_QUICKSLOT_4 = 21,

  ITEM_SLOT_NANO_AUG = 26,

  ITEM_SLOT_MURDER_CARD = 52,
};

}  // namespace Enum
}  // namespace FOMNetwork
