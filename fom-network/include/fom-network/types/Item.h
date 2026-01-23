#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct Item {};
#pragma pack(pop)

ASSERT_BLITTABLE(Item);

}  // namespace Type
}  // namespace FOMNetwork
