#pragma once

#include <fom-network/packets/models/EnemyUpdateModel.h>

#include "ModelSerializer.h"
#include "SignedWorldPlacementModelSerializer.h"

namespace FOMNetwork {

class EnemyUpdateModelSerializer
    : public ModelSerializer<Packet::EnemyUpdateModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::EnemyUpdateModel& model) const override {
    SignedWorldPlacementModelSerializer placementSerializer;

    bs.WriteCompressed(model.controllingPlayerID);
    bs.WriteCompressed(model.enemyID);
    bs.WriteCompressed(model.enemyType);
    placementSerializer.Write(bs, model.placement);

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
    SignedWorldPlacementModelSerializer placementSerializer;

    bs.ReadCompressed(model.controllingPlayerID);
    bs.ReadCompressed(model.enemyID);
    bs.ReadCompressed(model.enemyType);
    placementSerializer.Read(bs, model.placement);

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
