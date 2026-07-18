#pragma once

#include <fom-network/types/item/Item.h>

#include "../TypeSerializer.h"
#include "ItemBaseSerializer.h"

namespace FOMNetwork {
namespace Type {

class ItemSerializer : protected TypeSerializer<Type::Item> {
 public:
  void Write(RakNet::BitStream& bs, const Type::Item& data) const {
    bs.WriteCompressed(data.id);

    ItemBaseSerializer itemBaseSerializer;
    itemBaseSerializer.Write(bs, data.base);
  }

  bool Read(RakNet::BitStream& bs, Type::Item& data) const {
    if (!bs.ReadCompressed(data.id)) return false;

    ItemBaseSerializer itemBaseSerializer;
    if (!itemBaseSerializer.Read(bs, data.base)) return false;

    return true;
  }
};

}  // namespace Type
}  // namespace FOMNetwork
