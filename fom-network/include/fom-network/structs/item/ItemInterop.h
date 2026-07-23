#pragma once

#include <fom-network/Interop.h>
#include <fom-network/structs/item/ItemBaseInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ItemInterop {
  uint32_t id;
  ItemBaseInterop base;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemInterop);

}  // namespace FOMNetwork
