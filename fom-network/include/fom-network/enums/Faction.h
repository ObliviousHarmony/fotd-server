#pragma once

#include <fom-network/Common.h>

namespace FOMNetwork {
namespace Enums {

enum Faction : uint8_t {
  INVALID_FACTION = 0,
  LED = 1,
  FDC = 2,
  GOM = 3,
  BOS = 4,
  MOTB = 5,
  CMG = 6,
  EC = 7,
  VI = 8,
  NUM_FACTIONS = 9
};

}  // namespace Enums
}  // namespace FOMNetwork
