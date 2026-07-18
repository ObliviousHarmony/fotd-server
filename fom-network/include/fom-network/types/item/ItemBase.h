#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/item/ItemQuality.h>
#include <fom-network/enums/item/ItemSecurity.h>
#include <fom-network/enums/item/ItemType.h>

#include <cstring>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct ItemBase {
  Enum::ItemType type;
  uint16_t value;
  uint16_t maxDurability;
  uint16_t durability;
  uint8_t durabilityLossFactor;
  Enum::ItemSecurity security;
  uint32_t creatorPlayerId;
  uint32_t timeout;
  uint32_t stolenFromPlayerId;
  uint8_t classification;
  Enum::ItemQuality quality;
  uint8_t attributeBonus;
  uint8_t balanceValues[BufferSizes::NUM_ITEM_BALANCE_SLIDERS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemBase);

inline bool operator==(const ItemBase& a, const ItemBase& b) {
  return memcmp(&a, &b, sizeof(ItemBase)) == 0;
}

inline bool operator<(const ItemBase& a, const ItemBase& b) {
  return memcmp(&a, &b, sizeof(ItemBase)) < 0;
}

}  // namespace Type
}  // namespace FOMNetwork
