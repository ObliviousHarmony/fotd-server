#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/AttributeType.h>
#include <fom-network/structs/player/PlayerAttributesInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct AttributeChangePacket {
  int64_t changedMask;
  PlayerAttributesInterop attributes;
};
#pragma pack(pop)

ASSERT_BLITTABLE(AttributeChangePacket);

}  // namespace FOMNetwork
