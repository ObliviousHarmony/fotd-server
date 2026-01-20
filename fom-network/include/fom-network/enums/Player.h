#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum AvatarSex : uint8_t {
  MALE = 0,
  FEMALE = 1,
};

enum AvatarRace : uint8_t {
  WHITE = 0,
  BLACK = 1,
};

enum EquipmentSlot : uint8_t {
  HAT = 0,
  HEAD = 1,
  EYES = 2,
  SHOULDER = 3,
  ARMS = 4,
  TORSO = 5,
  BACK = 6,
  LEGS = 7,
  HANDS = 8,

  NUM_EQUIPMENT_SLOTS = 9
};

}  // namespace Enum
}  // namespace FOMNetwork
