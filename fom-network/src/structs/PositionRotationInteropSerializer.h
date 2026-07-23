#pragma once

#include <fom-network/structs/PositionRotationInterop.h>

#include "InteropTypeSerializer.h"
#include "PositionInteropSerializer.h"

namespace FOMNetwork {

class PositionRotationInteropSerializer
    : protected InteropTypeSerializer<PositionRotationInterop> {
 public:
  explicit PositionRotationInteropSerializer(uint32_t precision = 16)
      : precision_(precision) {}

  void Write(RakNet::BitStream& bs, const PositionRotationInterop& data) const {
    PositionInteropSerializer positionSerializer(precision_);

    positionSerializer.Write(bs, data.pos);
    WriteBits(bs, data.rot, 9);
  }

  bool Read(RakNet::BitStream& bs, PositionRotationInterop& data) const {
    PositionInteropSerializer positionSerializer(precision_);

    if (!positionSerializer.Read(bs, data.pos)) return false;
    if (!ReadBits(bs, data.rot, 9)) return false;

    return true;
  }

 private:
  uint32_t precision_;
};

}  // namespace FOMNetwork
