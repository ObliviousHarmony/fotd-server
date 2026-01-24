#pragma once

#include <cstring>

#include <fom-network/Interop.h>
#include <fom-network/enums/ItemQuality.h>
#include <fom-network/enums/ItemSecurity.h>
#include <fom-network/enums/ItemType.h>

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
  uint32_t creatorPlayerID;
  uint32_t timeout;
  uint32_t stolenFromPlayerID;
  uint8_t attributeBonus;
  Enum::ItemQuality quality;
  uint8_t classification;
  uint8_t balanceValues[4];
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
