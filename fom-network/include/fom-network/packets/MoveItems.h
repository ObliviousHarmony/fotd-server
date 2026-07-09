#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/ItemLocation.h>
#include <fom-network/enums/ItemSlot.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct MoveItems {
  uint32_t playerId;
  uint16_t numIds;
  uint32_t ids[BufferSizes::MAX_ITEM_LIST_SIZE];
  Enum::ItemLocation source;
  Enum::ItemLocation destination;
  Enum::ItemSlot sourceSlot;
  Enum::ItemSlot destinationSlot;
};
#pragma pack(pop)

ASSERT_BLITTABLE(MoveItems);

}  // namespace Packet
}  // namespace FOMNetwork
