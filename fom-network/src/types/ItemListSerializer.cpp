#include "ItemListSerializer.h"

#include <map>
#include <vector>

namespace FOMNetwork {

void ItemListSerializer::Write(RakNet::BitStream& bs,
                               const Type::ItemList& data) const {
  bs.WriteCompressed((uint16_t)0);
  bs.WriteCompressed((uint32_t)0);
  bs.WriteCompressed((uint32_t)0);
  bs.WriteCompressed((uint32_t)0);

  std::map<Type::ItemBase, std::vector<uint32_t>> stacks;
  for (uint32_t i = 0; i < data.itemCount; ++i) {
    stacks[data.items[i].base].push_back(data.items[i].id);
  }

  ItemBaseSerializer itemBaseSerializer;
  bs.WriteCompressed((uint16_t)stacks.size());
  for (const auto& stack : stacks) {
    itemBaseSerializer.Write(bs, stack.first);

    bs.WriteCompressed((uint16_t)stack.second.size());
    for (uint32_t id : stack.second) {
      bs.WriteCompressed(id);
    }
  }
}

bool ItemListSerializer::Read(RakNet::BitStream& bs,
                              Type::ItemList& data) const {
  uint16_t skip16;
  uint32_t skip32;
  if (!bs.ReadCompressed(skip16)) return false;
  if (!bs.ReadCompressed(skip32)) return false;
  if (!bs.ReadCompressed(skip32)) return false;
  if (!bs.ReadCompressed(skip32)) return false;

  uint16_t stackCount;
  if (!bs.ReadCompressed(stackCount)) return false;

  ItemBaseSerializer itemBaseSerializer;
  data.itemCount = 0;
  for (uint16_t i = 0; i < stackCount; ++i) {
    Type::ItemBase base;
    if (!itemBaseSerializer.Read(bs, base)) return false;

    uint16_t idCount;
    if (!bs.ReadCompressed(idCount)) return false;

    for (uint16_t j = 0; j < idCount; ++j) {
      if (data.itemCount >= BufferSizes::MAX_ITEM_LIST_SIZE) return false;

      if (!bs.ReadCompressed(data.items[data.itemCount].id)) return false;
      data.items[data.itemCount].base = base;
      ++data.itemCount;
    }
  }

  return true;
}

}  // namespace FOMNetwork
