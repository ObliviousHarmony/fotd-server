#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldID.h>
#include <fom-network/types/NetworkAddress.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PlayerMigrateWorld {
  uint32_t playerID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerMigrateWorld)

}  // namespace Packet
}  // namespace FOMNetwork
