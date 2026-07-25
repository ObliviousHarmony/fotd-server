#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum ItemRarity : uint8_t {
  ITEM_RARITY_STANDARD = 0,
  ITEM_RARITY_CUSTOM = 1,
  ITEM_RARITY_SPECIAL = 2,
  ITEM_RARITY_RARE = 3,
  ITEM_RARITY_SPECIAL_RARE = 4,

  NUM_ITEM_RARITIES = 5
};

}  // namespace Enum
}  // namespace FOMNetwork
