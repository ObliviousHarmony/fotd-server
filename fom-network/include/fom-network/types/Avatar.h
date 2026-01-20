#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/Player.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct Avatar {
  Enum::AvatarSex sex;
  Enum::AvatarRace race;
  uint16_t face;
  uint16_t hair;

  uint16_t factionID;
  uint16_t rankID;
  uint16_t legacyFactionID;

  uint16_t shirt;
  uint16_t bottoms;
  uint16_t shoes;
  uint16_t equipmentSlots[Enum::NUM_EQUIPMENT_SLOTS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(Avatar);

}  // namespace Type
}  // namespace FOMNetwork
