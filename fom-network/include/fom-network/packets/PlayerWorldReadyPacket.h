#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {

enum PlayerWorldReadyStatusPacket : uint8_t {
  PLAYER_WORLD_READY_SUCCESS = 0,
  PLAYER_WORLD_READY_PLAYER_NOT_FOUND = 1,
  PLAYER_WORLD_READY_PLAYER_ALREADY_CONNECTED = 2,
};

#pragma pack(push, 1)
struct PlayerWorldReadyPacket {
  uint32_t playerId;
  PlayerWorldReadyStatusPacket status;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerWorldReadyPacket);

}  // namespace FOMNetwork
