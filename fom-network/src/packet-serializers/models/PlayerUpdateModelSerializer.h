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

    bs.Write(model.verticalLookAngle);

    bs.Write(model.isAnimating == 1);
    if (model.isAnimating) WriteBits(bs, model.animationID, 12);

    bs.Write(model.isMoving == 1);
    if (model.isMoving) WriteBits(bs, model.movementStateID, 5);

    bs.Write(model.hasWeaponEquipped == 1);
    if (model.hasWeaponEquipped) {
      bs.ReadCompressed(model.equippedWeapon);
      bs.Write(model.isWeaponAimed == 1);
      bs.Write(model.isWeaponFiring == 1);
      if (model.isWeaponFiring) {
        WriteBits(bs, model.currentAmmo, 7);
        WriteBits(bs, model.firedPosX, 9);
        WriteBits(bs, model.firedPosY, 9);
        WriteBits(bs, model.firedPosZ, 9);

        bs.Write0();
        bs.Write0();
        bs.Write0();
      }
    }

    bs.Write(model.wasHit == 1);
    if (model.wasHit) {
      WriteBits(bs, model.hitAnimationID, 4);
      WriteBits(bs, model.hitDirection, 4);
    }

    bs.Write(model.isEmoting == 1);
    if (model.isEmoting) WriteBits(bs, model.emoteID, 6);

    bs.Write(model.hasAttachments == 1);
    if (model.hasAttachments) {
      for (int i = 0; i < Enums::NUM_ATTACHMENTS; ++i)
        bs.Write(model.isAttachmentEquipped[i] == 1);
      WriteBits(bs, model.activeAttachment, 5);
      if (model.activeAttachment == Enums::IMPLANT_SHIELD)
        WriteBits(bs, model.shieldSetting, 7);
    }

    bs.Write0();
    WriteBits(bs, 0, 8);
    WriteBits(bs, 0, 3);
    bs.WriteCompressed((uint8_t)0);
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

    bs.Read(model.verticalLookAngle);

    model.isAnimating = bs.ReadBit() ? 1 : 0;
    if (model.isAnimating) ReadBits(bs, model.animationID, 12);

    model.isMoving = bs.ReadBit() ? 1 : 0;
    if (model.isMoving) ReadBits(bs, model.movementStateID, 5);

    model.hasWeaponEquipped = bs.ReadBit() ? 1 : 0;
    if (model.hasWeaponEquipped) {
      bs.ReadCompressed(model.equippedWeapon);
      model.isWeaponAimed = bs.ReadBit() ? 1 : 0;
      model.isWeaponFiring = bs.ReadBit() ? 1 : 0;
      if (model.isWeaponFiring) {
        ReadBits(bs, model.currentAmmo, 7);
        ReadBits(bs, model.firedPosX, 9);
        ReadBits(bs, model.firedPosY, 9);
        ReadBits(bs, model.firedPosZ, 9);

        bs.IgnoreBits(1);
        bs.IgnoreBits(1);
        bs.IgnoreBits(1);
      }
    }

    model.wasHit = bs.ReadBit() ? 1 : 0;
    if (model.wasHit) {
      ReadBits(bs, model.hitAnimationID, 4);
      ReadBits(bs, model.hitDirection, 4);
    }

    model.isEmoting = bs.ReadBit() ? 1 : 0;
    if (model.isEmoting) ReadBits(bs, model.emoteID, 6);

    model.hasAttachments = bs.ReadBit() ? 1 : 0;
    if (model.hasAttachments) {
      for (int i = 0; i < Enums::NUM_ATTACHMENTS; ++i)
        model.isAttachmentEquipped[i] = bs.ReadBit() ? 1 : 0;
      ReadBits(bs, model.activeAttachment, 5);
      if (model.activeAttachment == Enums::IMPLANT_SHIELD)
        ReadBits(bs, model.shieldSetting, 7);
    }

    bs.IgnoreBits(1);
    bs.IgnoreBits(8);
    bs.IgnoreBits(3);

    uint8_t temp;
    bs.ReadCompressed(temp);
  }
};

}  // namespace FOMNetwork
