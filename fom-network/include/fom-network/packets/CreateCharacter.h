#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/Avatar.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct CreateCharacter {
  PlayerID_t playerID;
  Type::Avatar avatar;
  uint8_t name[20];
  uint8_t biography[511];
};
#pragma pack(pop)

ASSERT_BLITTABLE(CreateCharacter);

}  // namespace Packet
}  // namespace FOMNetwork
