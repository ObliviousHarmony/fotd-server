#include <fom-network/constants/PlayerConstants.h>
#include <fom-network/packets/RegisterClientReturn.h>

#include "../types/AvatarSerializer.h"
#include "../types/FactionEmblemSerializer.h"
#include "../types/FactionPerksSerializer.h"
#include "../types/ItemListSerializer.h"
#include "../types/ItemSerializer.h"
#include "../types/PlayerAttributesSerializer.h"
#include "../types/PlayerSkillsSerializer.h"
#include "../types/PositionSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {

void RegisterClientReturnSerializer::Write(
    RakNet::BitStream& bs, const Packet::RegisterClientReturn* data) const {
  ItemListSerializer itemListSerializer;
  ItemSerializer itemSerializer;
  AvatarSerializer avatarSerializer;
  PlayerAttributesSerializer attributesSerializer;
  PositionSerializer positionSerializer;
  FactionEmblemSerializer emblemSerializer;
  PlayerSkillsSerializer skillsSerializer;
  FactionPerksSerializer perksSerializer;

  bs.WriteCompressed(data->worldID);
  bs.WriteCompressed(data->playerID);
  bs.WriteCompressed(data->status);

  // Inventory (flattened)
  itemListSerializer.Write(bs, data->inventory);

  for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
    if (data->equipment[i].id != 0) {
      bs.Write1();
      itemSerializer.Write(bs, data->equipment[i]);
    } else {
      bs.Write0();
    }
  }

  for (int i = 0; i < Constants::NUM_WEAPON_SLOTS; ++i) {
    if (data->weapons[i].id != 0) {
      bs.Write1();
      itemSerializer.Write(bs, data->weapons[i]);
    } else {
      bs.Write0();
    }
  }

  // Nanomachine placeholder bits
  for (int i = 0; i < 6; ++i) bs.Write0();

  itemListSerializer.Write(bs, data->storage);

  // QuickSlots
  for (int i = 0; i < Constants::NUM_QUICK_SLOTS; ++i)
    bs.WriteCompressed(data->quickSlots[i]);

  // Avatar
  avatarSerializer.Write(bs, data->avatar);

  // PlayerAttributes
  attributesSerializer.Write(bs, data->attributes);

  // PlayerProfile (flattened)
  bs.WriteCompressed(data->profilePlayerID);
  if (data->profileField_0x4)
    bs.Write1();
  else
    bs.Write0();
  EncodeString(bs, data->profileName);
  EncodeString(bs, data->profileFactionName);
  EncodeString(bs, data->profileBiography);
  EncodeString(bs, data->profileRankName);

  bs.WriteCompressed(data->field_0xf28);
  bs.WriteCompressed(data->field_0xf29);

  // Avatar cache
  bs.WriteCompressed(data->avatarCacheCount);
  for (int i = 0; i < data->avatarCacheCount; ++i)
    avatarSerializer.Write(bs, data->avatarCache[i]);

  // field_0x49c4
  if (data->field_0x49c4)
    bs.Write1();
  else
    bs.Write0();

  // safezoneCenter
  positionSerializer.Write(bs, data->safezoneCenter);

  bs.WriteCompressed(data->safezoneRadius);
  bs.WriteCompressed(data->nodeID);

  // field_0x49e0
  if (data->field_0x49e0)
    bs.Write1();
  else
    bs.Write0();

  bs.WriteCompressed(data->cloningDuration);

  // FactionEmblem
  emblemSerializer.Write(bs, data->factionEmblem);

  EncodeString(bs, data->factionName);

  // PlayerSkills
  skillsSerializer.Write(bs, data->skills);

  // spawnPosition
  positionSerializer.Write(bs, data->spawnPosition);

  // spawnAtPosition
  if (data->spawnAtPosition)
    bs.Write1();
  else
    bs.Write0();

  // FactionPerks
  perksSerializer.Write(bs, data->factionPerks);
}

}  // namespace FOMNetwork
