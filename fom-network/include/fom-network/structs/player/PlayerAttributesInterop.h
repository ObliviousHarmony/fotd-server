#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/AttributeType.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct PlayerAttributesInterop {
  uint32_t values[Enum::NUM_ATTRIBUTE_TYPES];
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerAttributesInterop);

}  // namespace FOMNetwork
