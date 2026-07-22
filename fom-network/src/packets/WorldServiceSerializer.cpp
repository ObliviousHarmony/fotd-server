#include <fom-network/packets/WorldService.h>

#include "../types/faction/FactionEmblemSerializer.h"
#include "../types/item/ItemListSerializer.h"
#include "PacketSerializers.h"

namespace FOMNetwork {
namespace Packet {

bool WorldServiceSerializer::Read(RakNet::BitStream& bs,
                                  Packet::WorldService* data) const {
  Type::ItemListSerializer itemListSerializer;
  Type::FactionEmblemSerializer factionEmblemSerializer;

  uint8_t temp8 = 0;
  uint32_t temp32 = 0;
  uint8_t tempStr[32];
  tempStr[0] = '\0';
  Type::FactionEmblem tempEmblem;
  uint16_t temp16 = 0;

  if (!bs.ReadCompressed(data->playerId)) return false;
  if (!bs.ReadCompressed(data->action)) return false;

  switch (data->action) {
    case WORLD_SERVICE_ACTION_OPEN:
    case WORLD_SERVICE_ACTION_OPEN_UNKNOWN:
    case WORLD_SERVICE_ACTION_DEV:
    case WORLD_SERVICE_ACTION_MANAGER_REQUEST:
    case WORLD_SERVICE_ACTION_ACTIVATE_CONTROL:
    case WORLD_SERVICE_ACTION_PRISON_BAIL_REQUEST:
    case WORLD_SERVICE_ACTION_PRISON_MANAGER_REQUEST:
      if (!bs.ReadCompressed(data->id)) return false;
      break;

    case WORLD_SERVICE_ACTION_OPENED_STORAGE:
      if (!itemListSerializer.Read(bs, data->storage)) return false;
      break;

    case WORLD_SERVICE_ACTION_OPENED:
      if (!bs.ReadCompressed(data->serviceId)) return false;
      if (!bs.ReadCompressed(data->serviceType)) return false;
      if (!bs.ReadCompressed(temp8)) return false;
      if (!bs.ReadCompressed(temp8)) return false;
      if (!bs.ReadCompressed(temp32)) return false;
      if (!bs.ReadCompressed(temp32)) return false;
      if (!DecodeString(bs, tempStr)) return false;
      if (!factionEmblemSerializer.Read(bs, tempEmblem)) return false;
      if (!DecodeString(bs, tempStr)) return false;
      if (!DecodeString(bs, tempStr)) return false;
      if (!factionEmblemSerializer.Read(bs, tempEmblem)) return false;
      if (data->serviceType != Enum::WORLD_SERVICE_STORAGE_ACCESS) {
        for (int i = 0; i < 6; ++i) {
          if (!bs.ReadCompressed(temp16)) return false;
        }
      }

      break;

    case WORLD_SERVICE_ACTION_MANAGER_RETURN:
      // NOT IMPLEMENTED YET
      return false;

    case WORLD_SERVICE_ACTION_MANAGER_UPDATE:
      // NOT IMPLEMENTED YET
      return false;

    case WORLD_SERVICE_ACTION_OPENED_PRISONER:
      // NOT IMPLEMENTED YET
      return false;

    case WORLD_SERVICE_ACTION_PRISON_MANAGER_RELEASE:
    case WORLD_SERVICE_ACTION_PRISON_BAIL_SUBMIT:
      // NOT IMPLEMENTED YET
      return false;

    case WORLD_SERVICE_ACTION_PRISON_BAIL_RETURN:
      // NOT IMPLEMENTED YET
      return false;

    case WORLD_SERVICE_ACTION_PRISON_MANAGER_RETURN:
      // NOT IMPLEMENTED YET
      return false;

    case WORLD_SERVICE_ACTION_OPEN_CLONING:
    case WORLD_SERVICE_ACTION_CLOSE:
      break;

    default:
      return false;
  }

  return true;
}

void WorldServiceSerializer::Write(RakNet::BitStream& bs,
                                   const Packet::WorldService* data) const {
  Type::ItemListSerializer itemListSerializer;
  Type::FactionEmblemSerializer factionEmblemSerializer;

  uint8_t temp8 = 0;
  uint32_t temp32 = 0;
  uint8_t tempStr[32];
  tempStr[0] = '\0';
  Type::FactionEmblem tempEmblem;
  uint16_t temp16 = 0;

  bs.WriteCompressed(data->playerId);
  bs.WriteCompressed(data->action);

  switch (data->action) {
    case WORLD_SERVICE_ACTION_OPEN:
    case WORLD_SERVICE_ACTION_OPEN_UNKNOWN:
    case WORLD_SERVICE_ACTION_DEV:
    case WORLD_SERVICE_ACTION_MANAGER_REQUEST:
    case WORLD_SERVICE_ACTION_ACTIVATE_CONTROL:
    case WORLD_SERVICE_ACTION_PRISON_BAIL_REQUEST:
    case WORLD_SERVICE_ACTION_PRISON_MANAGER_REQUEST:
      bs.WriteCompressed(data->id);
      break;

    case WORLD_SERVICE_ACTION_OPENED_STORAGE:
      itemListSerializer.Write(bs, data->storage);
      break;

    case WORLD_SERVICE_ACTION_OPENED:
      bs.WriteCompressed(data->serviceId);
      bs.WriteCompressed(data->serviceType);
      bs.WriteCompressed(temp8);
      bs.WriteCompressed(temp8);
      bs.WriteCompressed(temp32);
      bs.WriteCompressed(temp32);
      EncodeString(bs, tempStr);
      factionEmblemSerializer.Write(bs, tempEmblem);
      EncodeString(bs, tempStr);
      EncodeString(bs, tempStr);
      factionEmblemSerializer.Write(bs, tempEmblem);
      if (data->serviceType != Enum::WORLD_SERVICE_STORAGE_ACCESS) {
        for (int i = 0; i < 6; ++i) {
          bs.WriteCompressed(temp16);
        }
      }
      break;

    case WORLD_SERVICE_ACTION_MANAGER_RETURN:
      // NOT IMPLEMENTED YET
      break;

    case WORLD_SERVICE_ACTION_MANAGER_UPDATE:
      // NOT IMPLEMENTED YET
      break;

    case WORLD_SERVICE_ACTION_OPENED_PRISONER:
      // NOT IMPLEMENTED YET
      break;

    case WORLD_SERVICE_ACTION_PRISON_MANAGER_RELEASE:
    case WORLD_SERVICE_ACTION_PRISON_BAIL_SUBMIT:
      // NOT IMPLEMENTED YET
      break;

    case WORLD_SERVICE_ACTION_PRISON_BAIL_RETURN:
      // NOT IMPLEMENTED YET
      break;

    case WORLD_SERVICE_ACTION_PRISON_MANAGER_RETURN:
      // NOT IMPLEMENTED YET
      break;

    case WORLD_SERVICE_ACTION_OPEN_CLONING:
    case WORLD_SERVICE_ACTION_CLOSE:
      break;
  }
}

}  // namespace Packet
}  // namespace FOMNetwork
