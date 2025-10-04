#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Packet {

enum PlayerEnteringWorldReturnStatus : uint8_t {
  PLAYER_ENTERING_WORLD_RETURN_ERROR = 0,
  PLAYER_ENTERING_WORLD_RETURN_READY = 1,
  PLAYER_ENTERING_WORLD_RETURN_SERVER_FULL = 2,
};

#pragma pack(push, 1)
struct PlayerEnteringWorldReturn {
  PlayerEnteringWorldReturnStatus status;
  uint32_t playerID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerEnteringWorldReturn);

}  // namespace Packet
}  // namespace FOMNetwork
