
#pragma once

#include <fom-network/Interop.h>
#include <fom-network/constants/PlayerConstants.h>
#include <fom-network/types/Avatar.h>
#include <fom-network/types/FactionEmblem.h>
#include <fom-network/types/FactionPerks.h>
#include <fom-network/types/Item.h>
#include <fom-network/types/ItemList.h>
#include <fom-network/types/PlayerAttributes.h>
#include <fom-network/types/PlayerProfile.h>
#include <fom-network/types/PlayerSkills.h>
#include <fom-network/types/PositionRotation.h>

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
  uint8_t worldId;
  uint32_t playerId;
  RegisterClientReturnStatus status;
  Type::ItemList inventory;
  Type::Item equipment[Constants::NUM_EQUIPMENT_SLOTS];
  Type::Item weapons[Constants::NUM_WEAPON_SLOTS];
  Type::Item activeConsumables[Constants::NUM_ACTIVE_CONSUMABLE_SLOTS];
  Type::Item
      nanomachineAugmentations[Constants::NUM_NANOMACHINE_AUGMENTATION_SLOTS];
  Type::ItemList storage;
  uint16_t quickslots[Constants::NUM_QUICKSLOTS];
  Type::Avatar avatar;
  Type::PlayerAttributes attributes;
  Type::PlayerProfile profile;
  uint8_t unknown1;
  uint8_t unknown2;
  uint16_t avatarCacheCount;
  Type::Avatar avatarCache[MAX_AVATAR_CACHE];
  uint8_t unknown3;
  Type::PositionRotation safezoneCenter;
  uint32_t safezoneRadius;
  uint32_t nodeId;
  uint8_t unknown4;
  uint16_t cloningDuration;
  Type::FactionEmblem factionEmblem;
  uint8_t factionName[BufferSizes::FACTION_NAME];
  Type::PlayerSkills skills;
  Type::PositionRotation spawnPosition;
  uint8_t spawnAtPosition;
  Type::FactionPerks factionPerks;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClientReturn);

}  // namespace Packet
}  // namespace FOMNetwork
