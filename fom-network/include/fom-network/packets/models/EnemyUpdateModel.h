#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/packets/models/SignedWorldPlacementModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct EnemyUpdateModel {
  PlayerID_t controllingPlayerID;
  uint32_t enemyID;
  uint16_t enemyType;
  SignedWorldPlacementModel placement;

  // ------------------ stateFlags == 0 ------------------
  uint8_t stateFlags;
  uint8_t wasHit;
  uint8_t movementState;
  uint8_t aiState;

  uint8_t isAttacking;
  uint8_t attackAnimation;  // isAttacking == 1
};
#pragma pack(pop)

ASSERT_BLITTABLE(EnemyUpdateModel);

}  // namespace Packet
}  // namespace FOMNetwork
