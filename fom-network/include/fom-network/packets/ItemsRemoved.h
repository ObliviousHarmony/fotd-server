#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Packet {

// Server -> client removal of items (by instance id) from a container. Id count
// is a u16 on the wire; the buffer is capped well above any realistic single
// removal batch.
constexpr uint32_t MAX_ITEMS_REMOVED = 255;

#pragma pack(push, 1)
struct ItemsRemoved {
  uint32_t playerId;
  uint8_t dest;
  uint16_t idCount;
  uint32_t ids[MAX_ITEMS_REMOVED];
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemsRemoved);

}  // namespace Packet
}  // namespace FOMNetwork
