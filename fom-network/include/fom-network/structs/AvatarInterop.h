#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/Avatar.h>
#include <fom-network/enums/item/ItemType.h>

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

  Enum::ItemType shirt;
  Enum::ItemType bottoms;
  Enum::ItemType shoes;
  Enum::ItemType hat;
  Enum::ItemType head;
  Enum::ItemType eyes;
  Enum::ItemType shoulder;
  Enum::ItemType arms;
  Enum::ItemType torso;
  Enum::ItemType back;
  Enum::ItemType legs;
  Enum::ItemType hands;

  uint8_t isCommander;
  uint8_t unknown2;
  uint8_t unknown3;
  uint8_t isGroupLeader;
};
#pragma pack(pop)

ASSERT_BLITTABLE(AvatarInterop);

}  // namespace FOMNetwork
