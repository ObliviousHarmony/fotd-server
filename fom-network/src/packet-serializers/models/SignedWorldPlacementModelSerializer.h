#pragma once

#include <fom-network/packets/models/SignedWorldPlacementModel.h>

#include "ModelSerializer.h"

namespace FOMNetwork {

class SignedWorldPlacementModelSerializer
    : public ModelSerializer<Packet::SignedWorldPlacementModel> {
 public:
  void Write(RakNet::BitStream& bs,
             const Packet::SignedWorldPlacementModel& model) const override {
    bs.WriteCompressed(model.x);
    bs.WriteCompressed(model.y);
    bs.WriteCompressed(model.z);
    WriteBits(bs, model.rotation, 9);
  }

  bool Read(RakNet::BitStream& bs,
            Packet::SignedWorldPlacementModel& model) const override {
    bs.ReadCompressed(model.x);
    bs.ReadCompressed(model.y);
    bs.ReadCompressed(model.z);
    ReadBits(bs, model.rotation, 9);
    return true;
  }
};

}  // namespace FOMNetwork
