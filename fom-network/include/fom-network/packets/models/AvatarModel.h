#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkEnums.h>

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
  Faction faction;
  uint16_t shirt;
  uint16_t bottoms;
  uint16_t shoes;
  uint16_t gloves;
};
#pragma pack(pop)

ASSERT_BLITTABLE(AvatarModel);

}  // namespace Packet
}  // namespace FOMNetwork
