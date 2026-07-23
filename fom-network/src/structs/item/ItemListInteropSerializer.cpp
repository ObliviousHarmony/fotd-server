#include "ItemListInteropSerializer.h"

#include <map>
#include <vector>

namespace FOMNetwork {

void ItemListInteropSerializer::Write(RakNet::BitStream& bs,
                                      const ItemListInterop& data) const {
  ItemBaseInteropSerializer itemBaseSerializer;

  uint32_t itemCount = data.itemCount;
  if (itemCount > BufferSizes::MAX_ITEM_LIST_SIZE)
    itemCount = BufferSizes::MAX_ITEM_LIST_SIZE;

  std::map<ItemBaseInterop, std::vector<uint32_t>> stacks;
  for (uint32_t i = 0; i < itemCount; ++i) {
    stacks[data.items[i].base].push_back(data.items[i].id);
  }

  bs.WriteCompressed(data.reservedSpace);
  bs.WriteCompressed(data.maxSpace);
  bs.WriteCompressed((uint32_t)100);
  bs.WriteCompressed((uint32_t)100);
  bs.WriteCompressed((uint16_t)stacks.size());
  for (const auto& stack : stacks) {
    itemBaseSerializer.Write(bs, stack.first);

    bs.WriteCompressed((uint16_t)stack.second.size());
    for (uint32_t id : stack.second) {
      bs.WriteCompressed(id);
    }
  }
}

bool ItemListInteropSerializer::Read(RakNet::BitStream& bs,
                                     ItemListInterop& data) const {
  ItemBaseInteropSerializer itemBaseSerializer;

  uint32_t skip32;
  if (!bs.ReadCompressed(data.reservedSpace)) return false;
  if (!bs.ReadCompressed(data.maxSpace)) return false;
  if (!bs.ReadCompressed(skip32)) return false;
  if (!bs.ReadCompressed(skip32)) return false;

  uint16_t stackCount;
  if (!bs.ReadCompressed(stackCount)) return false;

  data.itemCount = 0;
  for (uint16_t i = 0; i < stackCount; ++i) {
    ItemBaseInterop base;
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
