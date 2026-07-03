#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/Item.h>

namespace FOMNetwork {
namespace Packet {

// Server -> client update of existing items (matched by instance id). Unlike
// ItemsAdded this is a flat count + Item list (no dest, no stack grouping) and the
// count is a single byte, so it is capped at 255.
constexpr uint32_t MAX_ITEMS_CHANGED = 255;

#pragma pack(push, 1)
struct ItemsChanged {
  uint32_t playerId;
  uint8_t count;
  Type::Item items[MAX_ITEMS_CHANGED];
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemsChanged);

}  // namespace Packet
}  // namespace FOMNetwork
