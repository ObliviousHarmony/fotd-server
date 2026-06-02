#include "WorldUpdateSerializer.h"

namespace FOMNetwork {

namespace {

// TODO: mirror the client's avatar predicate (Ghidra FUN_100c8b60 /
// FUN_102575b0) that gates the implant block. Returning false skips the block,
// which round-trips with itself but will desync against a real client whose
// avatar carries implants.
bool HasImplantData(const Type::Avatar& /*avatar*/) { return false; }

// TODO: mirror the client's item-definition lookup (g_ItemDefTable[0x68]) that
// decides whether the equipped implant exposes a shield setting.
bool ImplantUsesShield(uint16_t /*activeImplants*/) { return false; }

}  // namespace

void WorldUpdateSerializer::Write(RakNet::BitStream& bs,
                                  const Type::WorldUpdate& data) const {
  bs.Write((uint8_t)data.type);

  switch (data.type) {
    case Type::WORLD_UPDATE_TYPE_PLAYER:
      WritePlayer(bs, data);
      break;
    case Type::WORLD_UPDATE_TYPE_CHARACTER:
      WriteCharacter(bs, data);
      break;
  }
}

bool WorldUpdateSerializer::Read(RakNet::BitStream& bs,
                                 Type::WorldUpdate& data) const {
  uint8_t type;
  if (!bs.Read(type)) return false;
  data.type = (Type::WorldUpdateType)type;

  switch (data.type) {
    case Type::WORLD_UPDATE_TYPE_PLAYER:
      return ReadPlayer(bs, data);
    case Type::WORLD_UPDATE_TYPE_CHARACTER:
      return ReadCharacter(bs, data);
  }

  return false;
}

void WorldUpdateSerializer::WritePlayer(RakNet::BitStream& bs,
                                        const Type::WorldUpdate& data) const {
  bs.WriteCompressed(data.grid1);
  bs.WriteCompressed(data.grid2);
  bs.WriteCompressed(data.visibilityAreaId);

  if (data.targetingTurretId != 0) {
    bs.Write(true);
    bs.WriteCompressed(data.targetingTurretId);
  } else {
    bs.Write(false);
  }

  if (data.activeMedicalTreatment != 0) {
    bs.Write(true);
    WriteBits(bs, data.activeMedicalTreatment, 3);
  } else {
    bs.Write(false);
  }

  bs.WriteCompressed(data.unknown1);

  WriteCharacter(bs, data);
}

bool WorldUpdateSerializer::ReadPlayer(RakNet::BitStream& bs,
                                       Type::WorldUpdate& data) const {
  if (!bs.ReadCompressed(data.grid1)) return false;
  if (!bs.ReadCompressed(data.grid2)) return false;
  if (!bs.ReadCompressed(data.visibilityAreaId)) return false;

  bool hasTargetingTurret;
  if (!bs.Read(hasTargetingTurret)) return false;
  if (hasTargetingTurret) {
    if (!bs.ReadCompressed(data.targetingTurretId)) return false;
  } else {
    data.targetingTurretId = 0;
  }

  bool hasMedicalTreatment;
  if (!bs.Read(hasMedicalTreatment)) return false;
  if (hasMedicalTreatment) {
    if (!ReadBits(bs, data.activeMedicalTreatment, 3)) return false;
  } else {
    data.activeMedicalTreatment = 0;
  }

  if (!bs.ReadCompressed(data.unknown1)) return false;

  return ReadCharacter(bs, data);
}

void WorldUpdateSerializer::WriteCharacter(RakNet::BitStream& bs,
                                           const Type::WorldUpdate& data) const {
  PositionRotationSerializer positionSerializer;
  AvatarSerializer avatarSerializer;
  PositionSerializer firedPositionSerializer;

  bs.WriteCompressed(data.id);
  positionSerializer.Write(bs, data.position);
  avatarSerializer.Write(bs, data.avatar);

  bs.Write(data.isDead == 1);
  if (data.isDead == 1) return;

  uint8_t verticalLook = (uint8_t)(data.verticalLookAngle + 90);
  WriteBits(bs, verticalLook, 8);

  if (data.animationId != 16) {
    bs.Write(true);
    WriteBits(bs, data.animationId, 12);
  } else {
    bs.Write(false);
  }

  if (data.movementStateId != 0) {
    bs.Write(true);
    WriteBits(bs, data.movementStateId, 5);
  } else {
    bs.Write(false);
  }

  if (data.equippedWeapon != 0) {
    bs.Write(true);
    bs.WriteCompressed(data.equippedWeapon);
    bs.Write(data.isWeaponAimed == 1);

    if (data.consumedAmmo != 0) {
      bs.Write(true);
      WriteBits(bs, data.consumedAmmo, 7);
    } else {
      bs.Write(false);
    }

    if (data.consumedAmmo != 0) {
      firedPositionSerializer.Write(bs, data.firedPosition);
    }
  } else {
    bs.Write(false);
  }

  bs.Write(data.wasHit == 1);
  if (data.wasHit == 1) {
    WriteBits(bs, data.hitAnimationId, 4);
    WriteBits(bs, data.hitDirection, 4);
  }

  if (data.emoteId != 0) {
    bs.Write(true);
    WriteBits(bs, data.emoteId, 6);
  } else {
    bs.Write(false);
  }

  if (HasImplantData(data.avatar)) {
    if (data.activeImplants != 0) {
      bs.Write(true);
      bs.WriteCompressed(data.activeImplants);
    } else {
      bs.Write(false);
    }
    if (ImplantUsesShield(data.activeImplants)) {
      WriteBits(bs, data.shieldSetting, 7);
    }
  }

  WriteBits(bs, data.movementSpeed, 8);
  WriteBits(bs, data.unknown2, 3);
  bs.Write(data.unknown3 == 1);
  WriteBits(bs, data.unknown4, 10);
  WriteBits(bs, data.unknown5, 10);
  bs.Write(data.isShieldActive == 1);
}

bool WorldUpdateSerializer::ReadCharacter(RakNet::BitStream& bs,
                                          Type::WorldUpdate& data) const {
  PositionRotationSerializer positionSerializer;
  AvatarSerializer avatarSerializer;
  PositionSerializer firedPositionSerializer;

  if (!bs.ReadCompressed(data.id)) return false;
  if (!positionSerializer.Read(bs, data.position)) return false;
  if (!avatarSerializer.Read(bs, data.avatar)) return false;

  bool isDead;
  if (!bs.Read(isDead)) return false;
  data.isDead = isDead ? 1 : 0;
  if (isDead) return true;

  uint8_t verticalLook;
  if (!ReadBits(bs, verticalLook, 8)) return false;
  data.verticalLookAngle = (int16_t)verticalLook - 90;

  bool hasAnimation;
  if (!bs.Read(hasAnimation)) return false;
  if (hasAnimation) {
    if (!ReadBits(bs, data.animationId, 12)) return false;
  } else {
    data.animationId = 16;
  }

  bool hasMovementState;
  if (!bs.Read(hasMovementState)) return false;
  if (hasMovementState) {
    if (!ReadBits(bs, data.movementStateId, 5)) return false;
  } else {
    data.movementStateId = 0;
  }

  bool hasWeapon;
  if (!bs.Read(hasWeapon)) return false;
  if (hasWeapon) {
    if (!bs.ReadCompressed(data.equippedWeapon)) return false;

    bool isWeaponAimed;
    if (!bs.Read(isWeaponAimed)) return false;
    data.isWeaponAimed = isWeaponAimed ? 1 : 0;

    bool hasConsumedAmmo;
    if (!bs.Read(hasConsumedAmmo)) return false;
    if (hasConsumedAmmo) {
      if (!ReadBits(bs, data.consumedAmmo, 7)) return false;
    } else {
      data.consumedAmmo = 0;
    }

    if (data.consumedAmmo != 0) {
      if (!firedPositionSerializer.Read(bs, data.firedPosition)) return false;
    }
  } else {
    data.equippedWeapon = 0;
  }

  bool wasHit;
  if (!bs.Read(wasHit)) return false;
  data.wasHit = wasHit ? 1 : 0;
  if (wasHit) {
    if (!ReadBits(bs, data.hitAnimationId, 4)) return false;
    if (!ReadBits(bs, data.hitDirection, 4)) return false;
  }

  bool hasEmote;
  if (!bs.Read(hasEmote)) return false;
  if (hasEmote) {
    if (!ReadBits(bs, data.emoteId, 6)) return false;
  } else {
    data.emoteId = 0;
  }

  if (HasImplantData(data.avatar)) {
    bool hasImplants;
    if (!bs.Read(hasImplants)) return false;
    if (hasImplants) {
      if (!bs.ReadCompressed(data.activeImplants)) return false;
    } else {
      data.activeImplants = 0;
    }
    if (ImplantUsesShield(data.activeImplants)) {
      if (!ReadBits(bs, data.shieldSetting, 7)) return false;
    }
  }

  if (!ReadBits(bs, data.movementSpeed, 8)) return false;
  if (!ReadBits(bs, data.unknown2, 3)) return false;

  bool unknown3;
  if (!bs.Read(unknown3)) return false;
  data.unknown3 = unknown3 ? 1 : 0;

  if (!ReadBits(bs, data.unknown4, 10)) return false;
  if (!ReadBits(bs, data.unknown5, 10)) return false;

  bool isShieldActive;
  if (!bs.Read(isShieldActive)) return false;
  data.isShieldActive = isShieldActive ? 1 : 0;

  return true;
}

}  // namespace FOMNetwork
