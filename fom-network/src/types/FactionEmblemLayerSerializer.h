#pragma once

#include <fom-network/types/FactionEmblemLayer.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class FactionEmblemLayerSerializer
    : protected TypeSerializer<Type::FactionEmblemLayer> {
 public:
  void Write(RakNet::BitStream& bs,
             const Type::FactionEmblemLayer& data) const {
    bool hasLayer =
        data.shape != 0 || data.offsetX != 0 || data.offsetY != 0;

    bs.Write(hasLayer);
    if (!hasLayer)
      return;
    bs.WriteCompressed(data.shape);
    bs.WriteCompressed(data.offsetX);
    bs.WriteCompressed(data.offsetY);
    WriteBits(bs, data.scaleWidth, 7);
    WriteBits(bs, data.scaleHeight, 7);
    WriteBits(bs, data.rotation, 9);
    bs.WriteCompressed(data.red);
    bs.WriteCompressed(data.green);
    bs.WriteCompressed(data.blue);
  }

  bool Read(RakNet::BitStream& bs, Type::FactionEmblemLayer& data) const {
    bool hasLayer;
    if (!bs.Read(hasLayer)) return false;
    if (!hasLayer) {
      data.shape = 0;
      data.offsetX = 0;
      data.offsetY = 0;
      data.scaleWidth = 0;
      data.scaleHeight = 0;
      data.rotation = 0;
      data.red = 0;
      data.green = 0;
      data.blue = 0;
      return true;
    }

    if (!bs.ReadCompressed(data.shape)) return false;
    if (!bs.ReadCompressed(data.offsetX)) return false;
    if (!bs.ReadCompressed(data.offsetY)) return false;

    data.scaleWidth = 0;
    data.scaleHeight = 0;
    data.rotation = 0;

    if (!ReadBits(bs, data.scaleWidth, 7)) return false;
    if (!ReadBits(bs, data.scaleHeight, 7)) return false;
    if (!ReadBits(bs, data.rotation, 9)) return false;

    if (!bs.ReadCompressed(data.red)) return false;
    if (!bs.ReadCompressed(data.green)) return false;
    if (!bs.ReadCompressed(data.blue)) return false;

    return true;
  }
};

}  // namespace FOMNetwork
