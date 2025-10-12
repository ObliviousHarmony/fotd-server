#pragma once

#include <fom-network/packets/models/PositionRotationModel.h>

#include "ModelSerializer.h"
#include "PositionModelSerializer.h"

namespace FOMNetwork {

class PositionRotationModelSerializer
    : public ModelSerializer<Packet::PositionRotationModel> {
 public:
  PositionRotationModelSerializer(int numPositionBits = 16) {
    posSerializer = PositionModelSerializer(numPositionBits);
  }

  void Write(RakNet::BitStream& bs,
             const Packet::PositionRotationModel& model) const override {
    posSerializer.Write(bs, model.position);
    WriteBits(bs, model.rotation, 9);
  }

  bool Read(RakNet::BitStream& bs,
            Packet::PositionRotationModel& model) const override {
    posSerializer.Read(bs, model.position);
    ReadBits(bs, model.rotation, 9);
    return true;
  }

 private:
  /* The number of bits to use when reading/writing the position. */
  PositionModelSerializer posSerializer;
};

}  // namespace FOMNetwork
