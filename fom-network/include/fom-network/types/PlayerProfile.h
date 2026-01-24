#pragma once

#include <fom-network/Interop.h>

namespace FOMNetwork {
namespace Type {

#pragma pack(push, 1)
struct PlayerProfile {
  uint32_t playerID;
  uint8_t unknown1;
  uint8_t playerName[BufferSizes::PLAYER_NAME];
  uint8_t factionName[BufferSizes::FACTION_NAME];
  uint8_t biography[BufferSizes::PLAYER_BIOGRAPHY];
  uint8_t rankName[BufferSizes::RANK_NAME];
};
#pragma pack(pop)

ASSERT_BLITTABLE(PlayerProfile);

}  // namespace Type
}  // namespace FOMNetwork
