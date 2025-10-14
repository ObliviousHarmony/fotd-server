#pragma once

#include <fom-network/packets/models/WorldUpdateModel.h>

#include "EnemyUpdateModelSerializer.h"
#include "ModelSerializer.h"
#include "PlayerUpdateModelSerializer.h"

namespace FOMNetwork {

class WorldUpdateModelSerializer
    : public ModelSerializer<Packet::WorldUpdateModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::WorldUpdateModel& model) const override {
    PlayerUpdateModelSerializer playerSerializer;
    EnemyUpdateModelSerializer enemySerializer;

    bs.WriteCompressed(model.type);
    switch (model.type) {
      case Enums::WORLDUPDATE_NEIGHBOR:

        playerSerializer.Write(bs, model.data.player);
        break;
      case Enums::WORLDUPDATE_NEIGHBOR_ENEMY:
        enemySerializer.Write(bs, model.data.enemy);
        break;
    }
  }

  bool Read(RakNet::BitStream& bs,
            Packet::WorldUpdateModel& model) const override {
    PlayerUpdateModelSerializer playerSerializer;
    EnemyUpdateModelSerializer enemySerializer;

    bs.ReadCompressed(model.type);
    switch (model.type) {
      case Enums::WORLDUPDATE_NEIGHBOR:

        playerSerializer.Read(bs, model.data.player);
        break;
      case Enums::WORLDUPDATE_NEIGHBOR_ENEMY:
        enemySerializer.Read(bs, model.data.enemy);
        break;
    }

    return true;
  }
};

}  // namespace FOMNetwork
