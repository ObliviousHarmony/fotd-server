#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/Avatar.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct CreateCharacter {
  uint32_t playerId;
  Type::Avatar avatar;
  uint8_t name[BufferSizes::PLAYER_NAME];
  uint8_t biography[BufferSizes::PLAYER_BIOGRAPHY];
};
#pragma pack(pop)

ASSERT_BLITTABLE(CreateCharacter);

}  // namespace Packet
}  // namespace FOMNetwork
