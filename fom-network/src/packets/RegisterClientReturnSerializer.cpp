#include <fom-network/constants/PlayerConstants.h>
#include <fom-network/packets/RegisterClientReturn.h>

#include "../types/AvatarSerializer.h"
#include "../types/FactionEmblemSerializer.h"
#include "../types/FactionPerksSerializer.h"
#include "../types/ItemListSerializer.h"
#include "../types/ItemSerializer.h"
#include "../types/PlayerAttributesSerializer.h"
#include "../types/PlayerProfileSerializer.h"
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
  PlayerProfileSerializer profileSerializer;
  FactionEmblemSerializer emblemSerializer;
  PlayerSkillsSerializer skillsSerializer;
  FactionPerksSerializer perksSerializer;

  bs.WriteCompressed(data->worldID);
  bs.WriteCompressed(data->playerID);
  bs.WriteCompressed(data->status);

  itemListSerializer.Write(bs, data->inventory);

  for (int i = 0; i < Enum::NUM_EQUIPMENT_SLOTS; ++i) {
    bs.Write(data->equipment[i].id != 0);
    if (data->equipment[i].id != 0)
      itemSerializer.Write(bs, data->equipment[i]);
  }

  for (int i = 0; i < Constants::NUM_WEAPON_SLOTS; ++i) {
    bs.Write(data->weapons[i].id != 0);
    if (data->weapons[i].id != 0) itemSerializer.Write(bs, data->weapons[i]);
  }

  for (int i = 0; i < Constants::NUM_UNKNOWN_ITEM_SLOTS; ++i) {
    bs.Write(data->unknownSlots[i].id != 0);
    if (data->unknownSlots[i].id != 0)
      itemSerializer.Write(bs, data->unknownSlots[i]);
  }

  itemListSerializer.Write(bs, data->storage);

  for (int i = 0; i < Constants::NUM_QUICK_SLOTS; ++i)
    bs.WriteCompressed(data->quickSlots[i]);

  avatarSerializer.Write(bs, data->avatar);
  attributesSerializer.Write(bs, data->attributes);
  profileSerializer.Write(bs, data->profile);

  bs.WriteCompressed(data->unknown1);
  bs.WriteCompressed(data->unknown2);

  bs.WriteCompressed(data->avatarCacheCount);
  for (int i = 0; i < data->avatarCacheCount; ++i)
    avatarSerializer.Write(bs, data->avatarCache[i]);

  bs.Write(data->unknown3 == 1);
  positionSerializer.Write(bs, data->safezoneCenter);
  bs.WriteCompressed(data->safezoneRadius);
  bs.WriteCompressed(data->nodeID);
  bs.Write(data->unknown4 == 1);
  bs.WriteCompressed(data->cloningDuration);
  emblemSerializer.Write(bs, data->factionEmblem);
  EncodeString(bs, data->factionName);
  skillsSerializer.Write(bs, data->skills);
  positionSerializer.Write(bs, data->spawnPosition);
  bs.Write(data->spawnAtPosition == 1);
  perksSerializer.Write(bs, data->factionPerks);
}

}  // namespace FOMNetwork
