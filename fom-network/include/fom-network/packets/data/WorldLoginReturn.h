#pragma once

#include <fom-network/Common.h>
#include <fom-network/enums/WorldID.h>
#include <fom-network/FOMNetworkTypes.h>

namespace FOMNetwork {
namespace Packet {

enum WorldLoginReturnStatus : uint8_t {
  WORLD_LOGIN_RETURN_INVALID = 0,
  WORLD_LOGIN_RETURN_SUCCESS = 1,
  WORLD_LOGIN_RETURN_SERVER_UNAVAILABLE = 2,
  WORLD_LOGIN_RETURN_FACTION_INACCESSIBLE = 3,
  WORLD_LOGIN_RETURN_SERVER_FULL = 4,
  WORLD_LOGIN_RETURN_FACTION_REVOKED = 5,
};

#pragma pack(push, 1)
struct WorldLoginReturn {
  WorldLoginReturnStatus status;
  Enums::WorldID worldID;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldLoginReturn);

}  // namespace Packet
}  // namespace FOMNetwork
