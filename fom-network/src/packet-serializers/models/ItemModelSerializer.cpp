#include "ItemModelSerializer.h"

#include <unordered_map>
#include <vector>

namespace FOMNetwork {

void ItemModelSerializer::WriteStacks(RakNet::BitStream& bs,
                                      const Packet::ItemModel* items,
                                      const int numItems) const {
  // An item stack is a list of item IDs that share the same type,
  // value, durability, and isFactionItem flag. In order for us
  // to transform our flat list into stacks, we need to
  // aggregate them first. We do this using a hash map
  // with the item properties encoded into the key.
  //
  // Most items of the same type will generally stack because many items
  // don't use the value or the durability. In cases where they do,
  // often there are only a few variations. In an effort to reduce
  // per-packet heap allocations, we will hold onto the vectors
  // as a thread static when we allocate them for new stack keys.
  thread_local std::unordered_map<uint64_t, std::vector<ItemID_t>> stacks;
  static thread_local size_t totalUniqueKeys = 0;
  // Clear each vector but keep them around for reuse.
  for (auto& entry : stacks) entry.second.clear();

  // Separate all of the items into stacks.
  for (int i = 0; i < numItems; ++i) {
    const auto& item = items[i];
    const uint64_t key = PackItemStackKey(item.type, item.value,
                                          item.durability, item.isFactionItem);

    // Allocate a new vector if this is
    // a stack we haven't seen before.
    auto it = stacks.find(key);
    if (it == stacks.end()) {
      it = stacks.emplace(key, std::vector<ItemID_t>()).first;
      ++totalUniqueKeys;
    }

    it->second.push_back(item.id);
  }

  // Now we can write all of the stacks into the BitStream.
  bs.WriteCompressed((uint16_t)stacks.size());
  for (const auto& entry : stacks) {
    const uint64_t key = entry.first;
    const std::vector<ItemID_t>& ids = entry.second;

    // The stack key encodes the shared item properties and
    // so we can unpack them to avoid needing to track the
    // stack properties separately.
    Enums::ItemType type;
    uint16_t value;
    uint16_t durability;
    uint8_t isFactionItem;
    UnpackItemStackKey(key, type, value, durability, isFactionItem);

    bs.WriteCompressed(type);
    bs.WriteCompressed(value);
    bs.WriteCompressed(durability);
    bs.Write(isFactionItem != 0);

    bs.WriteCompressed((uint16_t)ids.size());
    for (ItemID_t id : ids) bs.WriteCompressed(id);
  }

  // Don't accumulate too many unique stacks as it will waste memory.
  // This protects us from cases where durability or value have
  // resulted in a large variance of stacks.
  constexpr size_t kMaxCachedStacks = 16384;
  if (totalUniqueKeys > kMaxCachedStacks) {
    stacks.clear();
    totalUniqueKeys = 0;
  }
}

bool ItemModelSerializer::ReadStacks(RakNet::BitStream& bs, int maxItems,
                                     Packet::ItemModel* items,
                                     int& numItems) const {
  // Items are serialized as stacks, where each stack shares the same
  // type, value, durability, and isFactionItem flag. These are then
  // followed by a list of item IDs that belong to that stack. We
  // will read through each stack, unpack the shared properties,
  // and then add copies of the struct in the item buffer for
  // each of the item IDs in the stack.
  Packet::ItemModel tempItem;

  uint16_t numStacks;
  bs.ReadCompressed(numStacks);
  for (int i = 0; i < numStacks; ++i) {
    bs.ReadCompressed(tempItem.type);
    bs.ReadCompressed(tempItem.value);
    bs.ReadCompressed(tempItem.durability);
    tempItem.isFactionItem = bs.ReadBit() ? 1 : 0;

    uint16_t stackSize;
    bs.ReadCompressed(stackSize);
    for (int j = 0; j < stackSize; ++j) {
      bs.ReadCompressed(tempItem.id);

      // Once we've reached the maximum number of items, just
      // silently discard the rest and continue reading to
      // keep advancing the BitStream through the packet.
      if (numItems >= maxItems) {
        continue;
      }

      memcpy(&items[numItems++], &tempItem, sizeof(Packet::ItemModel));
    }
  }

  return true;
}

}  // namespace FOMNetwork
