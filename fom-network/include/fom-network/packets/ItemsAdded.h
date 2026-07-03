#pragma once

#include <fom-network/Interop.h>
#include <fom-network/types/ItemList.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ItemsAdded {
  uint32_t playerId;
  uint8_t dest;
  Type::ItemList items;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemsAdded);

}  // namespace Packet
}  // namespace FOMNetwork
