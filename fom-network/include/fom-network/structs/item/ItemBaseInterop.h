#pragma once

#include <fom-network/Interop.h>
#include <fom-network/enums/item/ItemRarity.h>
#include <fom-network/enums/item/ItemSecurity.h>
#include <fom-network/enums/item/ItemType.h>

#include <cstring>

namespace FOMNetwork {

#pragma pack(push, 1)
struct ItemBaseInterop {
  Enum::ItemType type;
  uint16_t value;
  uint16_t valueMax;
  uint16_t durability;
  uint8_t durabilityLossFactor;
  Enum::ItemSecurity security;
  uint32_t creatorPlayerId;
  uint32_t stolenFromPlayerId;
  uint32_t timeout;
  uint8_t recipeVariation;
  Enum::ItemRarity rarity;
  uint8_t attributeBonus;
  uint8_t recipeBalanceValues[BufferSizes::NUM_ITEM_BALANCE_SLIDERS];
};
#pragma pack(pop)

ASSERT_BLITTABLE(ItemBaseInterop);

inline bool operator==(const ItemBaseInterop& a, const ItemBaseInterop& b) {
  return memcmp(&a, &b, sizeof(ItemBaseInterop)) == 0;
}

inline bool operator<(const ItemBaseInterop& a, const ItemBaseInterop& b) {
  return memcmp(&a, &b, sizeof(ItemBaseInterop)) < 0;
}

}  // namespace FOMNetwork
