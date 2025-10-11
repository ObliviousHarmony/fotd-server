#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/Faction.h>

namespace FOMNetwork {
namespace Packet {

enum AvatarSex : uint8_t { Male, Female };
enum AvatarSkin : uint8_t { Light, Dark };

#pragma pack(push, 1)
struct AvatarModel {
  AvatarSex sex;
  AvatarSkin skinColor;
  uint8_t face;
  uint8_t hair;
  Enums::Faction faction;
  uint16_t shirt;
  uint16_t bottoms;
  uint16_t shoes;
  uint16_t gloves;

  uint8_t showArmor;
  uint16_t armorHead;      // showArmor == 1
  uint16_t armorGlasses;   // showArmor == 1
  uint16_t armorShoulder;  // showArmor == 1
  uint16_t armorArm;       // showArmor == 1
  uint16_t armorTorso;     // showArmor == 1
  uint16_t armorLeg;       // showArmor == 1

  uint8_t rank;
};
#pragma pack(pop)

ASSERT_BLITTABLE(AvatarModel);

}  // namespace Packet
}  // namespace FOMNetwork
