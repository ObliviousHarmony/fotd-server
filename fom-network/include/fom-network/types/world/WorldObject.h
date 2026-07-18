#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/item/ItemType.h>
#include <fom-network/types/PositionRotation.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct WorldObject {
  uint32_t id;
  Enum::ItemType itemType;
  uint8_t state;
  Type::PositionRotation position;
  uint32_t owningPlayerId;
};
#pragma pack(pop)

ASSERT_BLITTABLE(WorldObject);

}  // namespace Type
}  // namespace FOMNetwork
