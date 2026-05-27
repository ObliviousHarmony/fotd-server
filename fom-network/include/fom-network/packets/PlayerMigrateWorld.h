#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PlayerMigrateWorld {
  uint32_t playerId;
  uint32_t clientBinaryAddress;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerMigrateWorld)

}  // namespace Packet
}  // namespace FOMNetwork
