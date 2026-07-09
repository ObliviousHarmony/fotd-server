#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/ItemContainerType.h>
#include <fom-network/enums/ItemSlotType.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct MoveItems {
  uint32_t playerId;
  uint16_t numIds;
  uint32_t ids[BufferSizes::MAX_ITEM_LIST_SIZE];
  Enum::ItemContainerType from;
  Enum::ItemContainerType to;
  Enum::ItemSlotType fromSlot;
  Enum::ItemSlotType toSlot;
};
#pragma pack(pop)

ASSERT_BLITTABLE(MoveItems);

}  // namespace Packet
}  // namespace FOMNetwork
