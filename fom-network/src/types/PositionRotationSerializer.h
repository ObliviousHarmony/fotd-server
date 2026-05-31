#pragma once

#include <fom-network/types/PositionRotation.h>

#include "PositionSerializer.h"
#include "TypeSerializer.h"

namespace FOMNetwork {

class PositionRotationSerializer
    : protected TypeSerializer<Type::PositionRotation> {
 public:
  explicit PositionRotationSerializer(uint32_t precision = 16)
      : precision_(precision) {}

  void Write(RakNet::BitStream& bs, const Type::PositionRotation& data) const {
    PositionSerializer positionSerializer(precision_);

    positionSerializer.Write(bs, data.pos);
    WriteBits(bs, data.rot, 9);
  }

  bool Read(RakNet::BitStream& bs, Type::PositionRotation& data) const {
    PositionSerializer positionSerializer(precision_);

    if (!positionSerializer.Read(bs, data.pos)) return false;
    if (!ReadBits(bs, data.rot, 9)) return false;

    return true;
  }

 private:
  uint32_t precision_;
};

}  // namespace FOMNetwork
