#pragma once

#include <fom-network/Interop.h>
#include <fom-network/constants/PlayerConstants.h>
#include <fom-network/enums/Player.h>
#include <fom-network/types/Avatar.h>
#include <fom-network/types/FactionEmblem.h>
#include <fom-network/types/FactionPerks.h>
#include <fom-network/types/Item.h>
#include <fom-network/types/ItemList.h>
#include <fom-network/types/PlayerAttributes.h>
#include <fom-network/types/PlayerSkills.h>
#include <fom-network/types/Position.h>

namespace FOMNetwork {
namespace Packet {

enum RegisterClientReturnStatus : uint8_t {
  REGISTER_CLIENT_RETURN_SUCCESS = 1,
  REGISTER_CLIENT_RETURN_WORLD_FULL = 4,
  REGISTER_CLIENT_RETURN_INVALID_WORLD_FILE = 5,
};

constexpr int MAX_AVATAR_CACHE = 300;

#pragma pack(push, 1)
struct RegisterClientReturn {
  uint8_t worldID;
  uint32_t playerID;
  RegisterClientReturnStatus status;

  // Inventory (flattened)
  Type::ItemList inventory;
  Type::Item equipment[Enum::NUM_EQUIPMENT_SLOTS];
  Type::Item weapons[Constants::NUM_WEAPON_SLOTS];
  Type::ItemList storage;

  uint16_t quickSlots[Constants::NUM_QUICK_SLOTS];
  Type::Avatar avatar;
  Type::PlayerAttributes attributes;

  // PlayerProfile (flattened)
  uint32_t profilePlayerID;
  uint8_t profileField_0x4;  // unknown bit field
  uint8_t profileName[BufferSizes::PLAYER_NAME];
  uint8_t profileFactionName[BufferSizes::FACTION_NAME];
  uint8_t profileBiography[BufferSizes::PLAYER_BIOGRAPHY];
  uint8_t profileRankName[BufferSizes::RANK_NAME];

  uint8_t field_0xf28;  // unknown
  uint8_t field_0xf29;  // unknown
  uint16_t avatarCacheCount;
  Type::Avatar avatarCache[MAX_AVATAR_CACHE];

  uint8_t field_0x49c4;  // unknown bit field
  Type::Position safezoneCenter;
  uint32_t safezoneRadius;
  uint32_t nodeID;
  uint8_t field_0x49e0;  // unknown bit field
  uint16_t cloningDuration;

  Type::FactionEmblem factionEmblem;
  uint8_t factionName[BufferSizes::FACTION_NAME];

  Type::PlayerSkills skills;
  Type::Position spawnPosition;
  uint8_t spawnAtPosition;
  Type::FactionPerks factionPerks;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClientReturn);

}  // namespace Packet
}  // namespace FOMNetwork
