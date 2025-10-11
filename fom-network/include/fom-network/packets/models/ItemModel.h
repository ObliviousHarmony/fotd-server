#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/ItemType.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ItemModel {
  ItemID_t id;
  Enums::ItemType type;
  uint16_t value;
  uint16_t durability;
  uint8_t isFactionItem;
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemModel);

}  // namespace Packet
}  // namespace FOMNetwork
