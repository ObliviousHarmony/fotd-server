#pragma once

#include <fom-network/InteropTypes.h>

namespace FOMNetwork {
namespace Enum {

enum ChatChannel : uint8_t {
  CHAT_GENERAL = 0,
  CHAT_PRIVATE = 1,
  CHAT_FACTION = 2,
  CHAT_DOLLYINC = 3,
  CHAT_DEPARTMENT = 4,
  CHAT_MISSION = 5,
  CHAT_GLOBAL = 6,
  CHAT_HELP = 7,
  CHAT_STAFF = 8,
  CHAT_TRADE = 9,
  CHAT_SYSTEM = 10,
  CHAT_LOCAL = 11,
  CHAT_GM = 12,

  NUM_CHAT_CHANNELS = 13,
};

}  // namespace Enum
}  // namespace FOMNetwork
