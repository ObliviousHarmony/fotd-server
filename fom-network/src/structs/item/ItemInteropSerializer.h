#pragma once

#include <fom-network/structs/item/ItemInterop.h>

#include "../InteropTypeSerializer.h"
#include "ItemBaseInteropSerializer.h"

namespace FOMNetwork {

class ItemInteropSerializer : protected InteropTypeSerializer<ItemInterop> {
 public:
  void Write(RakNet::BitStream& bs, const ItemInterop& data) const {
    ItemBaseInteropSerializer itemBaseSerializer;

    bs.WriteCompressed(data.id);
    itemBaseSerializer.Write(bs, data.base);
  }

  bool Read(RakNet::BitStream& bs, ItemInterop& data) const {
    ItemBaseInteropSerializer itemBaseSerializer;

    if (!bs.ReadCompressed(data.id)) return false;
    if (!itemBaseSerializer.Read(bs, data.base)) return false;

    return true;
  }
};

}  // namespace FOMNetwork
