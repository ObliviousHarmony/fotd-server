#pragma once

#include <fom-network/types/Position.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class PositionSerializer : protected TypeSerializer<Type::Position> {
 public:
  explicit PositionSerializer(uint32_t precision = 16)
      : precision_(precision) {}

  void Write(RakNet::BitStream& bs, const Type::Position& data) const {
    if (precision_ > 15) {
      bs.WriteCompressed((uint16_t)data.x);
      bs.WriteCompressed((uint16_t)data.y);
      bs.WriteCompressed((uint16_t)data.z);
    } else {
      int32_t x = data.x < 0 ? -(int32_t)data.x : data.x;
      int32_t y = data.y < 0 ? -(int32_t)data.y : data.y;
      int32_t z = data.z < 0 ? -(int32_t)data.z : data.z;

      WriteBits(bs, x, precision_);
      WriteBits(bs, y, precision_);
      WriteBits(bs, z, precision_);

      bs.Write(data.x < 0);
      bs.Write(data.y < 0);
      bs.Write(data.z < 0);
    }

    WriteBits(bs, data.rot, 9);
  }

  bool Read(RakNet::BitStream& bs, Type::Position& data) const {
    if (precision_ > 15) {
      uint16_t x, y, z;
      if (!bs.ReadCompressed(x)) return false;
      if (!bs.ReadCompressed(y)) return false;
      if (!bs.ReadCompressed(z)) return false;

      data.x = (int16_t)x;
      data.y = (int16_t)y;
      data.z = (int16_t)z;
    } else {
      if (!ReadBits(bs, data.x, precision_)) return false;
      if (!ReadBits(bs, data.y, precision_)) return false;
      if (!ReadBits(bs, data.z, precision_)) return false;

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

 private:
  uint32_t precision_;
};

}  // namespace FOMNetwork
