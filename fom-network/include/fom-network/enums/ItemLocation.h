#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum ItemLocation : uint8_t {
  ITEM_LOCATION_INVALID = 0,

  ITEM_LOCATION_INVENTORY = 1,
  ITEM_LOCATION_EQUIPMENT = 2,
  ITEM_LOCATION_WEAPONS = 3,

  ITEM_LOCATION_QUICKSLOTS = 5,

  ITEM_LOCATION_DESTROY = 8,
};

}  // namespace Enum
}  // namespace FOMNetwork
