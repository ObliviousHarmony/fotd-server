#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/AttributeType.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct PlayerAttributes {
  uint32_t values[Enum::NUM_ATTRIBUTE_TYPES];
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerAttributes);

}  // namespace Type
}  // namespace FOMNetwork
