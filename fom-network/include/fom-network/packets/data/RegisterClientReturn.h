#pragma once

#include <fom-network/Common.h>
#include <fom-network/FOMNetworkTypes.h>
#include <fom-network/enums/WorldID.h>
#include <fom-network/enums/ItemType.h>
#include <fom-network/enums/EquipmentSlot.h>
#include <fom-network/packets/models/AvatarModel.h>
#include <fom-network/packets/models/ItemModel.h>
#include <fom-network/packets/models/ItemSlotModel.h>
#include <fom-network/packets/models/PlayerAttributesModel.h>

namespace FOMNetwork {
namespace Packet {

enum RegisterClientReturnStatus : uint8_t {
  REGISTER_CLIENT_RETURN_INVALID = 0,
  REGISTER_CLIENT_RETURN_SUCCESS = 1,
  REGISTER_CLIENT_RETURN_ERROR = 2,
  REGISTER_CLIENT_RETURN_WORLD_FULL = 4,
  REGISTER_CLIENT_RETURN_INTEGRITY_CHECK_FAILED = 5,
};

#pragma pack(push, 1)
struct RegisterClientReturn {
  Enums::WorldID worldID;
  PlayerID_t playerID;
  RegisterClientReturnStatus status;
  uint16_t numInventoryItems;
  ItemModel inventoryItemBuffer[500];
  ItemSlotModel equipmentSlots[Enums::NUM_EQUIPMENT_SLOTS];
  ItemSlotModel weaponSlots[NUM_WEAPON_SLOTS];
  Enums::ItemType quickSlots[NUM_QUICK_SLOTS];
  AvatarModel avatar;
  PlayerAttributesModel attributes;
  uint8_t name[20];
  uint8_t selectedNode;
};
#pragma pack(pop)

ASSERT_BLITTABLE(RegisterClientReturn);

}  // namespace Packet
}  // namespace FOMNetwork
