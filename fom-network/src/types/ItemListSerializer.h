#pragma once

#include <fom-network/types/ItemList.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class ItemListSerializer : protected TypeSerializer<Type::ItemList> {
 public:
  void Write(RakNet::BitStream& bs, const Type::ItemList& model) const {}
  bool Read(RakNet::BitStream& bs, Type::ItemList& model) const { return true; }
};

}  // namespace FOMNetwork
