#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/item/ItemContainerType.h>
#include <fom-network/enums/item/ItemSlotType.h>

namespace FOMNetwork {

#pragma pack(push, 1)
struct MoveItemsPacket {
  uint32_t playerId;
  uint16_t numItemIds;
  uint32_t itemIds[BufferSizes::MAX_ITEM_LIST_SIZE];
  Enum::ItemContainerType from;
  Enum::ItemContainerType to;
  Enum::ItemSlotType fromSlot;
  Enum::ItemSlotType toSlot;
};
#pragma pack(pop)

ASSERT_BLITTABLE(MoveItemsPacket);

}  // namespace FOMNetwork
