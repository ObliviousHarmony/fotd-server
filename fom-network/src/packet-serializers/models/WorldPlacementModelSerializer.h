#pragma once

#include <fom-network/packets/models/WorldPlacementModel.h>

#include "ModelSerializer.h"

namespace FOMNetwork {

class WorldPlacementModelSerializer
    : public ModelSerializer<Packet::WorldPlacementModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::WorldPlacementModel& model) const override {
    bs.WriteCompressed(model.x);
    bs.WriteCompressed(model.y);
    bs.WriteCompressed(model.z);
    WriteBits(bs, model.rotation, 9);
  }

  bool Read(RakNet::BitStream& bs,
            Packet::WorldPlacementModel& model) const override {
    bs.ReadCompressed(model.x);
    bs.ReadCompressed(model.y);
    bs.ReadCompressed(model.z);
    ReadBits(bs, model.rotation, 9);
    return true;
  }
};

}  // namespace FOMNetwork
