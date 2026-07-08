#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum UiItemSlot : uint8_t {
  UI_ITEM_SLOT_INVENTORY = 0,

  UI_ITEM_SLOT_WEAPON = 1,

  UI_ITEM_SLOT_HEAD = 5,
  UI_ITEM_SLOT_EYES = 6,
  UI_ITEM_SLOT_SHOULDERS = 7,
  UI_ITEM_SLOT_TORSO = 8,
  UI_ITEM_SLOT_ARMS = 9,
  UI_ITEM_SLOT_HANDS = 10,
  UI_ITEM_SLOT_LEGS = 11,
  UI_ITEM_SLOT_BACK = 12,
  UI_ITEM_SLOT_HAT = 13,

  UI_ITEM_SLOT_SHIRT = 14,
  UI_ITEM_SLOT_PANTS = 15,
  UI_ITEM_SLOT_SHOES = 16,

  UI_ITEM_SLOT_NANO_AUG = 26,

  UI_ITEM_SLOT_MURDER_CARD = 52,
};

}  // namespace Enum
}  // namespace FOMNetwork
