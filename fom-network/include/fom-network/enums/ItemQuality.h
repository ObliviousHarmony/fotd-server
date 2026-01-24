#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum ItemQuality : uint8_t {
  ITEM_QUALITY_STANDARD = 0,
  ITEM_QUALITY_CUSTOM = 1,
  ITEM_QUALITY_SPECIAL = 2,
  ITEM_QUALITY_RARE = 3,
  ITEM_QUALITY_SPECIAL_RARE = 4,

  NUM_ITEM_QUALITIES = 4
};

}  // namespace Enum
}  // namespace FOMNetwork
