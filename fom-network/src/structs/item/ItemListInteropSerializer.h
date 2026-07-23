#pragma once

#include <fom-network/structs/item/ItemListInterop.h>

#include "../InteropTypeSerializer.h"
#include "ItemBaseInteropSerializer.h"

namespace FOMNetwork {

class ItemListInteropSerializer
    : protected InteropTypeSerializer<ItemListInterop> {
 public:
  void Write(RakNet::BitStream& bs, const ItemListInterop& data) const;
  bool Read(RakNet::BitStream& bs, ItemListInterop& data) const;
};

}  // namespace FOMNetwork
