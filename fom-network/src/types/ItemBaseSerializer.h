#pragma once

#include <fom-network/types/ItemBase.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class ItemBaseSerializer : protected TypeSerializer<Type::ItemBase> {
 public:
  void Write(RakNet::BitStream& bs, const Type::ItemBase& model) const {}
  bool Read(RakNet::BitStream& bs, Type::ItemBase& model) const { return true; }
};

}  // namespace FOMNetwork
