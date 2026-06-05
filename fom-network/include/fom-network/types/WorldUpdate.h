#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/Avatar.h>
#include <fom-network/types/Position.h>
#include <fom-network/types/PositionRotation.h>

namespace FOMNetwork {
namespace Type {

enum WorldUpdateType : uint8_t {
  WORLD_UPDATE_TYPE_INVALID = 0,
  WORLD_UPDATE_TYPE_PLAYER = 1,
  WORLD_UPDATE_TYPE_CHARACTER = 2,
};

#pragma pack(push, 1)
struct WorldUpdate {
  WorldUpdateType kind;

  // ====== WORLD_UPDATE_TYPE_PLAYER ======
  uint32_t grid1;
  uint32_t grid2;
  uint8_t visibilityAreaId;
  uint32_t targetingTurretId;
  uint8_t activeMedicalTreatment;
  uint32_t unknown1;

  // ====== WORLD_UPDATE_TYPE_CHARACTER ======
  uint32_t id;
  PositionRotation position;
  Avatar avatar;
  uint8_t isDead;

  // === !isDead ===
  int16_t verticalLookAngle;
  uint16_t animationId;
  uint8_t movementStateId;

  uint16_t equippedWeapon;
  uint8_t isWeaponAimed;   // equippedWeapon != 0
  uint8_t consumedAmmo;    // equippedWeapon != 0
  Position firedPosition;  // consumedAmmo != 0

  uint8_t wasHit;
  uint8_t hitAnimationId;  // wasHit == 1
  uint8_t hitDirection;    // wasHit == 1

  uint8_t emoteId;

  uint16_t activeImplants;
  uint8_t shieldSetting;

  uint8_t movementSpeed;
  uint8_t unknown2;
  uint8_t unknown3;
  uint16_t unknown4;
  uint16_t unknown5;
  uint8_t isShieldActive;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldUpdate);

}  // namespace Type
}  // namespace FOMNetwork
