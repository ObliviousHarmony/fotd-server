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

  uint16_t factionId;
  uint16_t rankId;
  uint8_t unknown1;  // 6 bits on wire
  uint16_t legacyFactionId;

  uint16_t equipmentSlots[Enum::NUM_EQUIPMENT_SLOTS];

  uint8_t isCommander;    // 1 bit on wire
  uint8_t unknown2;       // 1 bit on wire
  uint8_t unknown3;       // 1 bit on wire
  uint8_t isGroupLeader;  // 1 bit on wire
};
#pragma pack(pop)

ASSERT_BLITTABLE(Avatar);

}  // namespace Type
}  // namespace FOMNetwork
