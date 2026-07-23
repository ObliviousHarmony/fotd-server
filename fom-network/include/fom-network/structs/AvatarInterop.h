#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/Avatar.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct AvatarInterop {
  Enum::AvatarSex sex;
  Enum::AvatarRace race;
  uint16_t face;
  uint16_t hair;

  uint16_t factionId;
  uint16_t rankId;
  uint16_t unknown1;
  uint16_t legacyFactionId;

  uint16_t shirt;
  uint16_t bottoms;
  uint16_t shoes;
  uint16_t hat;
  uint16_t head;
  uint16_t eyes;
  uint16_t shoulder;
  uint16_t arms;
  uint16_t torso;
  uint16_t back;
  uint16_t legs;
  uint16_t hands;

  uint8_t isCommander;
  uint8_t unknown2;
  uint8_t unknown3;
  uint8_t isGroupLeader;
};
#pragma pack(pop)

ASSERT_BLITTABLE(AvatarInterop);

}  // namespace FOMNetwork
