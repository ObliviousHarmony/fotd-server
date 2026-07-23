#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/item/ItemInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ItemListInterop {
  uint16_t reservedSpace;
  uint32_t maxSpace;
  uint32_t itemCount;
  ItemInterop items[BufferSizes::MAX_ITEM_LIST_SIZE];
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemListInterop);

}  // namespace FOMNetwork
