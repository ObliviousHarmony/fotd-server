#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct ItemBase {};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemBase);

}  // namespace Type
}  // namespace FOMNetwork
