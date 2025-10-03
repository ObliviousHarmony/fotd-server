#pragma once

#include <fom-network/Common.h>
#include <fom-network/packets/models/AvatarModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct CreateCharacter {
  uint32_t playerID;
  AvatarModel avatar;
  uint8_t name[20];
  uint8_t biography[511];
};
#pragma pack(pop)

ASSERT_BLITTABLE(CreateCharacter);

}  // namespace Packet
}  // namespace FOMNetwork
