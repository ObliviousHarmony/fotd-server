#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/WorldId.h>
#include <fom-network/types/NetworkAddress.h>

namespace FOMNetwork {
namespace Packet {

enum WorldLoginReturnStatus : uint8_t {
  WORLD_LOGIN_RETURN_INVALID = 0,
  WORLD_LOGIN_RETURN_SUCCESS = 1,
  WORLD_LOGIN_RETURN_SERVER_OFFLINE = 2,
  WORLD_LOGIN_RETURN_WRONG_FACTION = 3,
  WORLD_LOGIN_RETURN_WORLD_FULL = 4,
  WORLD_LOGIN_RETURN_UNKNOWN_ERROR = 5,
  WORLD_LOGIN_RETURN_NO_FACTION_PRIVILEGES = 6,
  WORLD_LOGIN_RETURN_OUT_OF_RANGE = 7,
  WORLD_LOGIN_RETURN_RETRY = 8,
};

#pragma pack(push, 1)
struct WorldLoginReturn {
  WorldLoginReturnStatus status;
  Enum::WorldId worldId;
  Type::NetworkAddress worldServerAddress;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldLoginReturn);

}  // namespace Packet
}  // namespace FOMNetwork
