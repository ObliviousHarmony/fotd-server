#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/AvatarInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct CreateCharacterPacket {
  uint32_t playerId;
  AvatarInterop avatar;
  uint8_t name[BufferSizes::PLAYER_NAME];
  uint8_t biography[BufferSizes::PLAYER_BIOGRAPHY];
};
#pragma pack(pop)

ASSERT_BLITTABLE(CreateCharacterPacket);

}  // namespace FOMNetwork
