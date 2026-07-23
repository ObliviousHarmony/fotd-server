
#pragma once

#include <fom-network/Interop.h>
#include <fom-network/constants/PlayerConstants.h>
#include <fom-network/structs/AvatarInterop.h>
#include <fom-network/structs/PositionRotationInterop.h>
#include <fom-network/structs/faction/FactionEmblemInterop.h>
#include <fom-network/structs/faction/FactionPerksInterop.h>
#include <fom-network/structs/item/ItemInterop.h>
#include <fom-network/structs/item/ItemListInterop.h>
#include <fom-network/structs/player/PlayerAttributesInterop.h>
#include <fom-network/structs/player/PlayerProfileInterop.h>
#include <fom-network/structs/player/PlayerSkillsInterop.h>

namespace FOMNetwork {

enum RegisterClientReturnStatus : uint8_t {
  REGISTER_CLIENT_RETURN_SUCCESS = 1,
  REGISTER_CLIENT_RETURN_WORLD_FULL = 4,
  REGISTER_CLIENT_RETURN_INVALID_WORLD_FILE = 5,
};

constexpr int MAX_AVATAR_CACHE = 300;

#pragma pack(push, 1)
struct RegisterClientReturnPacket {
  uint8_t worldId;
  uint32_t playerId;
  RegisterClientReturnStatus status;
  ItemListInterop inventory;
  ItemInterop equipment[Constants::NUM_EQUIPMENT_SLOTS];
  ItemInterop weapons[Constants::NUM_WEAPON_SLOTS];
  ItemInterop activeConsumables[Constants::NUM_ACTIVE_CONSUMABLE_SLOTS];
  ItemInterop
      nanomachineAugmentations[Constants::NUM_NANOMACHINE_AUGMENTATION_SLOTS];
  ItemListInterop storage;
  uint16_t quickslots[Constants::NUM_QUICKSLOTS];
  AvatarInterop avatar;
  PlayerAttributesInterop attributes;
  PlayerProfileInterop profile;
  uint8_t unknown1;
  uint8_t unknown2;
  uint16_t avatarCacheCount;
  AvatarInterop avatarCache[MAX_AVATAR_CACHE];
  uint8_t unknown3;
  PositionRotationInterop safezoneCenter;
  uint32_t safezoneRadius;
  uint32_t nodeId;
  uint8_t unknown4;
  uint16_t cloningDuration;
  FactionEmblemInterop factionEmblem;
  uint8_t factionName[BufferSizes::FACTION_NAME];
  PlayerSkillsInterop skills;
  PositionRotationInterop spawnPosition;
  uint8_t spawnAtPosition;
  FactionPerksInterop factionPerks;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClientReturnPacket);

}  // namespace FOMNetwork
