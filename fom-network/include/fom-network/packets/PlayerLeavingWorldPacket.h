#pragma once

#include <fom-network/Interop.h>
#include <fom-network/NetworkAddress.h>
#include <fom-network/enums/WorldId.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct PlayerLeavingWorldPacket {
  uint32_t playerId;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerLeavingWorldPacket);

}  // namespace FOMNetwork
