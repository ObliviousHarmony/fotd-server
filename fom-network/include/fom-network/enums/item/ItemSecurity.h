#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum ItemSecurity : uint8_t {
  ITEM_SECURITY_NORMAL = 0,
  ITEM_SECURITY_SECURED = 1,
  ITEM_SECURITY_BOUND = 2,
  ITEM_SECURITY_SPECIAL_BOUND = 3,

  NUM_ITEM_SECURITIES = 4
};

}  // namespace Enum
}  // namespace FOMNetwork
