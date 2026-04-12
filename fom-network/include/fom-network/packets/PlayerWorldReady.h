#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

enum PlayerWorldReadyStatus : uint8_t {
  PLAYER_WORLD_READY_SUCCESS = 0,
  PLAYER_WORLD_READY_PLAYER_NOT_FOUND = 1,
};

#pragma pack(push, 1)
struct PlayerWorldReady {
  uint32_t playerID;
  PlayerWorldReadyStatus status;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerWorldReady);

}  // namespace Packet
}  // namespace FOMNetwork
