#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/AvatarInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct AvatarChangePacket {
  uint32_t playerId;
  AvatarInterop avatar;
};
#pragma pack(pop)

ASSERT_BLITTABLE(AvatarChangePacket);

}  // namespace FOMNetwork
