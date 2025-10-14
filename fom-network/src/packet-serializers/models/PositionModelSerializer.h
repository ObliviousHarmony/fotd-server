#pragma once

#include <fom-network/packets/models/PositionModel.h>

#include "ModelSerializer.h"

namespace FOMNetwork {

class PositionModelSerializer : public ModelSerializer<Packet::PositionModel> {
 public:
  PositionModelSerializer(int numPositionBits = 16)
      : numPositionBits(numPositionBits) {}

  void Write(RakNet::BitStream& bs,
             const Packet::PositionModel& model) const override {
    if (numPositionBits >= 16) {
      bs.WriteCompressed((uint16_t)model.x);
      bs.WriteCompressed((uint16_t)model.y);
      bs.WriteCompressed((uint16_t)model.z);
      return;
    }

    WriteBits(bs, model.x, numPositionBits);
    WriteBits(bs, model.y, numPositionBits);
    WriteBits(bs, model.z, numPositionBits);

    // Anything less than 16 loses the sign bit, so we
    // need to write it separately.
    bs.Write(model.x < 0);
    bs.Write(model.y < 0);
    bs.Write(model.z < 0);
  }

  bool Read(RakNet::BitStream& bs,
            Packet::PositionModel& model) const override {
    if (numPositionBits >= 16) {
      bs.ReadCompressed((uint16_t&)model.x);
      bs.ReadCompressed((uint16_t&)model.y);
      bs.ReadCompressed((uint16_t&)model.z);
      return true;
    }

    ReadBits(bs, model.x, numPositionBits);
    ReadBits(bs, model.y, numPositionBits);
    ReadBits(bs, model.z, numPositionBits);

    // Restore the sign bits.
    if (bs.ReadBit()) model.x = -model.x;
    if (bs.ReadBit()) model.y = -model.y;
    if (bs.ReadBit()) model.z = -model.z;

    return true;
  }

 private:
  /* The number of bits to use when reading/writing the position. */
  int numPositionBits;
};

}  // namespace FOMNetwork
