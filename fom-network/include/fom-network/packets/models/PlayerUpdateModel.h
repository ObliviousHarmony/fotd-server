#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/ItemType.h>
#include <fom-network/enums/PlayerAttachment.h>
#include <fom-network/packets/models/AvatarModel.h>
#include <fom-network/packets/models/WorldPlacementModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PlayerUpdateModel {
  PlayerID_t playerID;
  WorldPlacementModel placement;  // Might need to be signed?
  AvatarModel avatar;

  uint8_t isDead;

  // ------------------ isDead == 0 ------------------
  uint8_t verticalLookAngle;

  uint8_t isAnimating; // Not Needed! animationID == 16 does a zero bit, otherwise it's a 1 and the ID
  uint16_t animationID;  // isAnimating == 1

  uint8_t isMoving; // Not needed! movement state ID of 0 means do nothing, otherwise, write 5 bits?
  uint8_t movementStateID;  // isMoving == 1

  uint8_t hasWeaponEquipped;  // Not needed! equippedWeapon of 0 means no
                              // weapon, otherwise it's a 1 and the weapon ID
  Enums::ItemType equippedWeapon;  // hasWeaponEquipped == 1
  uint8_t isWeaponAimed;           // hasWeaponEquipped == 1
  uint8_t isWeaponFiring;          // hasWeaponEquipped == 1
  uint8_t currentAmmo;             // isWeaponFiring == 1 // AMMO USED, isWeaponFiring does nothing!
  uint16_t firedPosX;              // isWeaponFiring == 1 // currentAmmo > 0
  uint16_t firedPosY;              // isWeaponFiring == 1 // currentAmmo > 0
  uint16_t firedPosZ;              // isWeaponFiring == 1 // currentAmmo > 0

  uint8_t wasHit;
  uint8_t hitAnimationID;  // wasHit = 1
  uint8_t hitDirection;    // wasHit = 1

  uint8_t isEmoting; // Not needed, emoteID of 0 means no emote
  uint8_t emoteID;  // isEmoting = 1

  uint8_t hasAttachments; // Not needed, see if any are equipped and write a 1 if they are, otherwise, write a zero
  uint8_t isAttachmentEquipped[Enums::NUM_ATTACHMENTS];  // hasAttachments == 1
  Enums::PlayerAttachment activeAttachment;              // hasAttachments == 1
  uint8_t shieldSetting;  // activeAttachment == Enums::IMPLANT_SHIELD
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerUpdateModel);

}  // namespace Packet
}  // namespace FOMNetwork
