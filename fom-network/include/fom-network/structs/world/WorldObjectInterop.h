#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/item/ItemType.h>
#include <fom-network/structs/PositionRotationInterop.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct WorldObjectInterop {
  uint32_t id;
  Enum::ItemType itemType;
  uint8_t state;
  PositionRotationInterop position;
  uint32_t owningPlayerId;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldObjectInterop);

}  // namespace FOMNetwork
