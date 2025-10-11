#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/packets/models/ItemModel.h>

namespace FOMNetwork {
namespace Packet {

#pragma pack(push, 1)
struct ItemSlotModel {
  uint8_t inUse;
  ItemModel item;  // inUse == 1
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemSlotModel);

}  // namespace Packet
}  // namespace FOMNetwork
