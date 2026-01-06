#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum AccountType : uint8_t {
  ACCOUNT_TYPE_INVALID = 0,
  ACCOUNT_TYPE_FREE = 1,
  ACCOUNT_TYPE_PREPAID = 2,
  ACCOUNT_TYPE_SUBSCRIPTION = 3,

  NUM_ACCOUNT_TYPES  // Unknown
};

}  // namespace Enum
}  // namespace FOMNetwork
