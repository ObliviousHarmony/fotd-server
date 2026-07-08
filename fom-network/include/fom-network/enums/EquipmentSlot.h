#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum EquipmentSlot : uint8_t {
  EQUIPMENT_SLOT_HEAD = 0,
  EQUIPMENT_SLOT_EYES = 1,
  EQUIPMENT_SLOT_SHOULDERS = 2,
  EQUIPMENT_SLOT_TORSO = 3,
  EQUIPMENT_SLOT_ARMS = 4,
  EQUIPMENT_SLOT_HANDS = 5,
  EQUIPMENT_SLOT_LEGS = 6,
  EQUIPMENT_SLOT_BACK = 7,
  EQUIPMENT_SLOT_HAT = 8,
  EQUIPMENT_SLOT_SHIRT = 9,
  EQUIPMENT_SLOT_PANTS = 10,
  EQUIPMENT_SLOT_SHOES = 11,

  NUM_EQUIPMENT_SLOTS = 12,
};

}  // namespace Enum
}  // namespace FOMNetwork
