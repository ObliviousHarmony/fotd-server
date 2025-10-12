#pragma once

#include <fom-network/packets/models/EnemyUpdateModel.h>

#include "ModelSerializer.h"
#include "PositionRotationModelSerializer.h"

namespace FOMNetwork {

class EnemyUpdateModelSerializer
    : public ModelSerializer<Packet::EnemyUpdateModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::EnemyUpdateModel& model) const override {
    PositionRotationModelSerializer positionRotationSerializer;

    bs.WriteCompressed(model.controllingPlayerID);
    bs.WriteCompressed(model.enemyID);
    bs.WriteCompressed(model.enemyType);
    positionRotationSerializer.Write(bs, model.positionRotation);

    WriteBits(bs, model.stateFlags, 8);
    if (model.stateFlags == 0) {
      bs.Write(model.wasHit == 1);
      WriteBits(bs, model.movementState, 5);
      WriteBits(bs, model.aiState, 4);
      bs.Write(model.isAttacking == 1);
      if (model.isAttacking) WriteBits(bs, model.attackAnimation, 6);
    }
  }

  bool Read(RakNet::BitStream& bs,
            Packet::EnemyUpdateModel& model) const override {
    PositionRotationModelSerializer positionRotationSerializer;

    bs.ReadCompressed(model.controllingPlayerID);
    bs.ReadCompressed(model.enemyID);
    bs.ReadCompressed(model.enemyType);
    positionRotationSerializer.Read(bs, model.positionRotation);

    ReadBits(bs, model.stateFlags, 8);
    if (model.stateFlags == 0) {
      model.wasHit = bs.ReadBit() ? 1 : 0;
      ReadBits(bs, model.movementState, 5);
      ReadBits(bs, model.aiState, 4);
      model.isAttacking = bs.ReadBit() ? 1 : 0;
      if (model.isAttacking) ReadBits(bs, model.attackAnimation, 6);
    }
  }
};

}  // namespace FOMNetwork
