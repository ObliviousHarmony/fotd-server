#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct CreateCharacter {
  uint8_t name[20];
  uint8_t biography[511];
  Avatar avatar;
};
#pragma pack(pop)

ASSERT_BLITTABLE(CreateCharacter);

}  // namespace Packet
}  // namespace FOMNetwork
