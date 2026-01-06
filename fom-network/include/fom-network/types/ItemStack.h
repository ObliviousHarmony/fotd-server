#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct ItemStack {};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemStack);

}  // namespace Type
}  // namespace FOMNetwork
