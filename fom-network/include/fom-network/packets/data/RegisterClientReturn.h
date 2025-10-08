#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/packets/models/AvatarModel.h>

namespace FOMNetwork {
namespace Packet {

enum RegisterClientReturnStatus : uint8_t {
  REGISTER_CLIENT_RETURN_INVALID = 0,
  REGISTER_CLIENT_RETURN_SUCCESS = 1,
  REGISTER_CLIENT_RETURN_ERROR = 2,
  REGISTER_CLIENT_RETURN_WORLD_FULL = 4,
  REGISTER_CLIENT_RETURN_INTEGRITY_CHECK_FAILED = 5,
};

#pragma pack(push, 1)
struct RegisterClientReturn {
  WorldID worldID;
  PlayerID_t playerID;
  RegisterClientReturnStatus status;
  AvatarModel avatar;
  uint32_t health;
  uint32_t stamina;
  uint32_t bioEnergy;
  uint32_t aura;
  uint32_t credits;
  uint32_t factionCredits;
  uint32_t experiencePoints;
  uint32_t penaltyPoints;
  uint32_t availableClones;
  uint8_t name[20];
  uint8_t selectedNode;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClientReturn);

}  // namespace Packet
}  // namespace FOMNetwork
