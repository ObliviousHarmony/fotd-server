#pragma once

#include <fom-network/packets/models/PlayerUpdateModel.h>

#include "AvatarModelSerializer.h"
#include "ModelSerializer.h"
#include "WorldPlacementModelSerializer.h"

namespace FOMNetwork {

class PlayerUpdateModelSerializer
    : public ModelSerializer<Packet::PlayerUpdateModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::PlayerUpdateModel& model) const override {
    WorldPlacementModelSerializer placementSerializer;
    AvatarModelSerializer avatarSerializer;

    bs.WriteCompressed(model.playerID);
    placementSerializer.Write(bs, model.placement);
    avatarSerializer.Write(bs, model.avatar);

    bs.Write(model.isDead == 1);
    if (model.isDead) return;

    uint8_t verticalLookAngle =
        (uint8_t)(model.verticalLookAngle + 90);  // Offset to avoid negative
    bs.Write(verticalLookAngle);

    // Don't write the default animation.
    if (model.animationID == 16) {
      bs.Write0();
    } else {
      bs.Write1();
      WriteBits(bs, model.animationID, 12);
    }

    if (model.movementStateID) {
      bs.Write1();
      WriteBits(bs, model.movementStateID, 5);
    } else
      bs.Write0();

    if (model.equippedWeapon != 0) {
      bs.Write1();
      bs.WriteCompressed(model.equippedWeapon);
      bs.Write(model.isWeaponAimed == 1);

      if (model.consumedAmmo) {
        bs.Write1();
        WriteBits(bs, model.consumedAmmo, 7);
      } else
        bs.Write0();

      if (model.consumedAmmo) {
        WriteBits(bs, model.firedPosX, 9);
        WriteBits(bs, model.firedPosY, 9);
        WriteBits(bs, model.firedPosZ, 9);
      }
    } else
      bs.Write0();

    bs.Write(model.wasHit == 1);
    if (model.wasHit) {
      WriteBits(bs, model.hitAnimationID, 4);
      WriteBits(bs, model.hitDirection, 4);
    }

    if (model.emoteID) {
      bs.Write1();
      WriteBits(bs, model.emoteID, 6);
    } else
      bs.Write0();

    bool hasAttachmentEquipped = false;
    for (int i = 0; i < Enums::NUM_ATTACHMENTS; ++i) {
      if (model.isAttachmentEquipped[i]) {
        hasAttachmentEquipped = true;
        break;
      }
    }
    if (hasAttachmentEquipped) {
      bs.Write1();
      for (int i = 0; i < Enums::NUM_ATTACHMENTS; ++i)
        bs.Write(model.isAttachmentEquipped[i] == 1);
      WriteBits(bs, model.activeAttachment, 5);
      if (model.activeAttachment == Enums::IMPLANT_SHIELD)
        WriteBits(bs, model.shieldSetting, 7);
    } else
      bs.Write0();

    bs.Write0();
    WriteBits(bs, 0, 8);
    WriteBits(bs, 0, 3);
  }

  bool Read(RakNet::BitStream& bs,
            Packet::PlayerUpdateModel& model) const override {
    WorldPlacementModelSerializer placementSerializer;
    AvatarModelSerializer avatarSerializer;

    bs.ReadCompressed(model.playerID);
    placementSerializer.Read(bs, model.placement);
    avatarSerializer.Read(bs, model.avatar);

    model.isDead = bs.ReadBit() ? 1 : 0;
    if (model.isDead) return true;

    uint8_t verticalLookAngle;
    bs.Read(verticalLookAngle);
    model.verticalLookAngle =
        (int8_t)(verticalLookAngle)-90;  // Reverse Write Offset

    if (bs.ReadBit())
      ReadBits(bs, model.animationID, 12);
    else
      model.animationID = 16;  // Default Animation (standing idle)

    if (bs.ReadBit())
      ReadBits(bs, model.movementStateID, 5);
    else
      model.movementStateID = 0;

    if (bs.ReadBit()) {
      bs.ReadCompressed(model.equippedWeapon);
      model.isWeaponAimed = bs.ReadBit() ? 1 : 0;

      if (bs.ReadBit()) {
        ReadBits(bs, model.consumedAmmo, 7);
      }

      if (model.consumedAmmo) {
        ReadBits(bs, model.firedPosX, 9);
        ReadBits(bs, model.firedPosY, 9);
        ReadBits(bs, model.firedPosZ, 9);

        // These aren't mystery bits, _duh_!
        // They're the < 16 bits case for position encoding!
        // They indicate an axis flip (x, y, z)!
        // One challenge though is what this does to the variables?
        // They're _supposed_ to be unsigned, right? Maybe they're not? Maybe they're always _signed_ and
        // this is just a strange bug that we're seeing here? Does the client get mad if I use a signed
        // type and spawn the player at the wrong position? This generally makes sense anyway because
        // the player can be at a negative position in the world, right?
        uint8_t bit1 = bs.ReadBit() ? 1 : 0;
        uint8_t bit2 = bs.ReadBit() ? 1 : 0;
        uint8_t bit3 = bs.ReadBit() ? 1 : 0;
      } else
        model.consumedAmmo = 0;
    }

    model.wasHit = bs.ReadBit() ? 1 : 0;
    if (model.wasHit) {
      ReadBits(bs, model.hitAnimationID, 4);
      ReadBits(bs, model.hitDirection, 4);
    }

    if (bs.ReadBit()) {
      ReadBits(bs, model.emoteID, 6);
    } else
      model.emoteID = 0;

    if (bs.ReadBit()) {
      for (int i = 0; i < Enums::NUM_ATTACHMENTS; ++i)
        model.isAttachmentEquipped[i] = bs.ReadBit() ? 1 : 0;
      ReadBits(bs, model.activeAttachment, 5);
      if (model.activeAttachment == Enums::IMPLANT_SHIELD)
        ReadBits(bs, model.shieldSetting, 7);
    }

    // Some missing state check?
    if (bs.ReadBit()) {
    }

    bs.IgnoreBits(8);
    bs.IgnoreBits(3);

    printf("Remaining Unread Bits: %u\n", bs.GetNumberOfUnreadBits());

    return true;
  }
};

}  // namespace FOMNetwork
