#pragma once

#include <fom-network/types/ItemStack.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class ItemStackSerializer : protected TypeSerializer<Type::ItemStack> {
 public:
  void Write(RakNet::BitStream& bs, const Type::ItemStack& model) const {}
  bool Read(RakNet::BitStream& bs, Type::ItemStack& model) const {
    return true;
  }
};

}  // namespace FOMNetwork
