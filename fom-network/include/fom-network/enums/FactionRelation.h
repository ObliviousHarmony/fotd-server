#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Enums {

enum FactionRelation : uint8_t {
  INVALID_RELATION = 0,
  ALLY = 1,
  ECONOMIC_ALLY = 2,
  NEUTRAL = 3,
  ECONOMIC_ENEMY = 4,
  ENEMY = 5,
  NUM_RELATIONS = 6
};

}  // namespace Enums
}  // namespace FOMNetwork
