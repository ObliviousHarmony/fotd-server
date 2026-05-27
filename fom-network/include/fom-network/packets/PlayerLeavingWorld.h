#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldId.h>
#include <fom-network/types/NetworkAddress.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PlayerLeavingWorld {
  uint32_t playerId;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerLeavingWorld);

}  // namespace Packet
}  // namespace FOMNetwork
