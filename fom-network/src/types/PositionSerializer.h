#pragma once

#include <fom-network/types/Position.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class PositionSerializer : protected TypeSerializer<Type::Position> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Position& data) const {
    if (data.precision > 15) {
      bs.WriteCompressed(data.x);
      bs.WriteCompressed(data.y);
      bs.WriteCompressed(data.z);
    } else {
      int16_t x = data.x < 0 ? -data.x : data.x;
      int16_t y = data.y < 0 ? -data.y : data.y;
      int16_t z = data.z < 0 ? -data.z : data.z;

      WriteBits(bs, x, data.precision);
      WriteBits(bs, y, data.precision);
      WriteBits(bs, z, data.precision);

      bs.Write(data.x < 0);
      bs.Write(data.y < 0);
      bs.Write(data.z < 0);
    }

    WriteBits(bs, data.rot, 9);
  }

  bool Read(RakNet::BitStream& bs, Type::Position& data) const {
    if (data.precision > 15) {
      if (!bs.ReadCompressed(data.x)) return false;
      if (!bs.ReadCompressed(data.y)) return false;
      if (!bs.ReadCompressed(data.z)) return false;
    } else {
      if (!ReadBits(bs, data.x, data.precision)) return false;
      if (!ReadBits(bs, data.y, data.precision)) return false;
      if (!ReadBits(bs, data.z, data.precision)) return false;

      bool negX, negY, negZ;
      if (!bs.Read(negX)) return false;
      if (!bs.Read(negY)) return false;
      if (!bs.Read(negZ)) return false;
      if (negX) data.x = -data.x;
      if (negY) data.y = -data.y;
      if (negZ) data.z = -data.z;
    }

    if (!ReadBits(bs, data.rot, 9)) return false;

    return true;
  }
};

}  // namespace FOMNetwork
