#pragma once

#include <fom-network/types/ItemList.h>

#include "ItemBaseSerializer.h"
#include "TypeSerializer.h"

namespace FOMNetwork {
namespace Type {

class ItemListSerializer : protected TypeSerializer<Type::ItemList> {
 public:
  void Write(RakNet::BitStream& bs, const Type::ItemList& data) const;
  bool Read(RakNet::BitStream& bs, Type::ItemList& data) const;
};

}  // namespace Type
}  // namespace FOMNetwork
