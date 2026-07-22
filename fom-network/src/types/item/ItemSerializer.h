#pragma once

#include <fom-network/types/item/Item.h>

#include "../TypeSerializer.h"
#include "ItemBaseSerializer.h"

namespace FOMNetwork {
namespace Type {

class ItemSerializer : protected TypeSerializer<Type::Item> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Item& data) const {
    ItemBaseSerializer itemBaseSerializer;

    bs.WriteCompressed(data.id);
    itemBaseSerializer.Write(bs, data.base);
  }

  bool Read(RakNet::BitStream& bs, Type::Item& data) const {
    ItemBaseSerializer itemBaseSerializer;

    if (!bs.ReadCompressed(data.id)) return false;
    if (!itemBaseSerializer.Read(bs, data.base)) return false;

    return true;
  }
};

}  // namespace Type
}  // namespace FOMNetwork
