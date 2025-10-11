#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Enums {

enum WorldUpdateType : uint8_t {
  WORLDUPDATE_INVALID = 0,
  WORLDUPDATE_PLAYER = 1,
  WORLDUPDATE_NEIGHBOR = 2,
  WORLDUPDATE_ENEMY = 3,
  WORLDUPDATE_NEIGHBOR_ENEMY = 4,
};

}  // namespace Enums
}  // namespace FOMNetwork
