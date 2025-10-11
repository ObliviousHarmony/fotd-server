#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/ItemType.h>
#include <fom-network/enums/PlayerAttachment.h>

#include "../models/AvatarModel.h"
#include "../models/WorldPlacementModel.h"

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PlayerUpdateModel {
  PlayerID_t playerID;
  WorldPlacementModel placement;
  AvatarModel avatar;

  uint8_t isDead;

  // ------------------ isDead == 0 ------------------
  uint8_t verticalLookAngle;

  uint8_t isAnimating;
  uint16_t animationID;  // isAnimating == 1

  uint8_t isMoving;
  uint8_t movementStateID;  // isMoving == 1

  uint8_t hasWeaponEquipped;
  Enums::ItemType equippedWeapon;  // hasWeaponEquipped == 1
  uint8_t isWeaponAimed;           // hasWeaponEquipped == 1
  uint8_t isWeaponFiring;          // hasWeaponEquipped == 1
  uint8_t currentAmmo;             // isWeaponFiring == 1
  uint16_t firedPosX;              // isWeaponFiring == 1
  uint16_t firedPosY;              // isWeaponFiring == 1
  uint16_t firedPosZ;              // isWeaponFiring == 1

  uint8_t wasHit;
  uint8_t hitAnimationID;  // wasHit = 1
  uint8_t hitDirection;    // wasHit = 1

  uint8_t isEmoting;
  uint8_t emoteID;  // isEmoting = 1

  uint8_t hasAttachments;
  uint8_t isAttachmentEquipped[Enums::NUM_ATTACHMENTS];  // hasAttachments == 1
  Enums::PlayerAttachment activeAttachment;              // hasAttachments == 1
  uint8_t shieldSetting;  // activeAttachment == Enums::IMPLANT_SHIELD
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerUpdateModel);

}  // namespace Packet
}  // namespace FOMNetwork
