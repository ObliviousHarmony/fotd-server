#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PlayerEnteringWorld {
  PlayerID_t playerID;
  uint8_t selectedNodeID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerEnteringWorld);

}  // namespace Packet
}  // namespace FOMNetwork
