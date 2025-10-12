#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/ItemType.h>
#include <fom-network/enums/PlayerAttachment.h>
#include <fom-network/packets/models/AvatarModel.h>
#include <fom-network/packets/models/PositionModel.h>
#include <fom-network/packets/models/PositionRotationModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct PlayerUpdateModel {
  PlayerID_t playerID;
  PositionRotationModel positionRotation;
  AvatarModel avatar;

  uint8_t isDead;

  // ------------------ isDead == 0 ------------------
  int8_t verticalLookAngle;
  uint16_t animationID = 16;  // Default Animation (standing idle)
  uint8_t movementStateID;

  Enums::ItemType equippedWeapon;
  uint8_t isWeaponAimed;    // equippedWeapon != 0
  uint8_t consumedAmmo;     // equippedWeapon != 0
  PositionModel firedFrom;  // consumedAmmo > 0

  uint8_t wasHit;
  uint8_t hitAnimationID;  // wasHit = 1
  uint8_t hitDirection;    // wasHit = 1

  uint8_t emoteID;
  uint8_t isAttachmentEquipped[Enums::NUM_ATTACHMENTS];
  Enums::PlayerAttachment activeAttachment;  // isAttachmentEquipped[n] != 0
  uint8_t shieldSetting;  // activeAttachment == Enums::IMPLANT_SHIELD
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerUpdateModel);

}  // namespace Packet
}  // namespace FOMNetwork
