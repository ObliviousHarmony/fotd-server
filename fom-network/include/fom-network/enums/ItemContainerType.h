#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum ItemContainerType : uint8_t {
  ITEM_CONTAINER_INVALID = 0,

  ITEM_CONTAINER_INVENTORY = 1,
  ITEM_CONTAINER_EQUIPMENT = 2,
  ITEM_CONTAINER_WEAPONS = 3,

  ITEM_CONTAINER_QUICKSLOTS = 5,

  ITEM_CONTAINER_DESTROY = 8,
};

}  // namespace Enum
}  // namespace FOMNetwork
