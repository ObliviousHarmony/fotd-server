#pragma once

#include <fom-network/types/item/ItemList.h>

#include "../TypeSerializer.h"
#include "ItemBaseSerializer.h"

namespace FOMNetwork {
namespace Type {

class ItemListSerializer : protected TypeSerializer<Type::ItemList> {
 public:
  void Write(RakNet::BitStream& bs, const Type::ItemList& data) const;
  bool Read(RakNet::BitStream& bs, Type::ItemList& data) const;
};

}  // namespace Type
}  // namespace FOMNetwork
