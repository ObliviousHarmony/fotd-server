#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/item/ItemBase.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct Item {
  uint32_t id;
  Type::ItemBase base;
};
#pragma pack(pop)

ASSERT_BLITTABLE(Item);

}  // namespace Type
}  // namespace FOMNetwork
