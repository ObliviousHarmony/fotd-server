#pragma once

#include <fom-network/types/Item.h>

#include "TypeSerializer.h"

namespace FOMNetwork {

class ItemSerializer : protected TypeSerializer<Type::Item> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Item& data) const {}

  bool Read(RakNet::BitStream& bs, Type::Item& data) const { return true; }
};

}  // namespace FOMNetwork
