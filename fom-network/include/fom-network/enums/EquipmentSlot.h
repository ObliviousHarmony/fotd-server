#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum EquipmentSlot : uint8_t {
  EQUIPMENT_SLOT_SHIRT = 0,
  EQUIPMENT_SLOT_BOTTOMS = 1,
  EQUIPMENT_SLOT_SHOES = 2,
  EQUIPMENT_SLOT_HAT = 3,
  EQUIPMENT_SLOT_HEAD = 4,
  EQUIPMENT_SLOT_EYES = 5,
  EQUIPMENT_SLOT_SHOULDER = 6,
  EQUIPMENT_SLOT_ARMS = 7,
  EQUIPMENT_SLOT_TORSO = 8,
  EQUIPMENT_SLOT_BACK = 9,
  EQUIPMENT_SLOT_LEGS = 10,
  EQUIPMENT_SLOT_HANDS = 11,

  NUM_EQUIPMENT_SLOTS = 12
};

}  // namespace Enum
}  // namespace FOMNetwork
