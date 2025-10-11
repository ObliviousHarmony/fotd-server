#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Enums {

enum EquipmentSlot : uint8_t {
  SLOT_HEAD = 0,
  SLOT_FACE = 1,
  SLOT_SHOULDER = 2,
  SLOT_CHEST = 3,
  SLOT_ARMS = 4,
  SLOT_HANDS = 5,
  SLOT_LEGS = 6,
  SLOT_BACK = 7,
  SLOT_HAT = 8,
  SLOT_SHIRT = 9,
  SLOT_BOTTOMS = 10,
  SLOT_SHOES = 11,
  NUM_EQUIPMENT_SLOTS = 12
};

}  // namespace Enums
}  // namespace FOMNetwork
