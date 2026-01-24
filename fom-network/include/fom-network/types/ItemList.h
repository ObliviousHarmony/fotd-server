#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/Item.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct ItemList {
  uint32_t itemCount;
  Item items[BufferSizes::MAX_ITEM_LIST_SIZE];
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemList);

}  // namespace Type
}  // namespace FOMNetwork
