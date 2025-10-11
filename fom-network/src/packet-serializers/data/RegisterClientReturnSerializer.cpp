#include <fom-network/enums/EquipmentSlot.h>
#include <fom-network/packets/PacketSerializers.h>

#include "../models/AvatarModelSerializer.h"
#include "../models/ItemModelSerializer.h"
#include "../models/ItemSlotModelSerializer.h"
#include "../models/PlayerAttributesModelSerializer.h"

namespace FOMNetwork {

void RegisterClientReturnSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::RegisterClientReturn& data) const {
  ItemModelSerializer itemSerializer;
  ItemSlotModelSerializer itemSlotSerializer;
  AvatarModelSerializer avatarSerializer;
  PlayerAttributesModelSerializer attributesSerializer;

  bs.WriteCompressed(data.worldID);
  bs.WriteCompressed(data.playerID);
  bs.WriteCompressed(data.status);

  itemSerializer.WriteStacks(bs, data.inventoryItemBuffer,
                             data.numInventoryItems);
  for (int i = 0; i < Enums::NUM_EQUIPMENT_SLOTS; ++i) {
    itemSlotSerializer.Write(bs, data.equipmentSlots[i]);
  }
  for (int i = 0; i < NUM_WEAPON_SLOTS; ++i) {
    itemSlotSerializer.Write(bs, data.weaponSlots[i]);
  }
  for (int i = 0; i < NUM_QUICK_SLOTS; ++i) {
    bs.WriteCompressed(data.quickSlots[i]);
  }

  avatarSerializer.Write(bs, data.avatar);
  attributesSerializer.Write(bs, data.attributes);

  // Unknown
  bs.WriteCompressed((uint32_t)0);
  bs.Write1();
  bs.Write1();

  EncodeString(bs, data.name);
  bs.WriteCompressed(
      (uint32_t)0);  // Department Name, Null Terminator (No name)
  bs.WriteCompressed((uint32_t)0);  // Unknown String, Null Terminator

  bs.WriteCompressed((uint8_t)0);  // World Owner
  bs.WriteCompressed((uint8_t)0);  // World Owner Relation

  bs.WriteCompressed(data.selectedNode);

  // Players?
  bs.WriteCompressed((uint16_t)0);

  // World Objects
  bs.WriteCompressed((uint16_t)0);

  bs.Write0();  // Unknown Bit

  // Mining/Production Processes
  for (int i = 0; i < 4; ++i) {
    bs.WriteCompressed((uint32_t)0);
    bs.WriteCompressed((uint16_t)0);  // Item Type
    bs.WriteCompressed((uint8_t)0);   // Cooling %
    bs.WriteCompressed((uint8_t)0);   // Heating %
    bs.WriteCompressed((uint8_t)0);
    bs.WriteCompressed((uint8_t)0);
    bs.Write0();
    bs.WriteCompressed((uint8_t)0);  // quantity of units completed
    bs.WriteCompressed((uint8_t)0);  // quantity of units queued
    bs.Write0();                     // Paused
    bs.WriteCompressed((uint8_t)0);
    bs.WriteCompressed((uint32_t)0);  // Base Cost Per Unit
    bs.WriteCompressed((uint8_t)0);   // Tax Rate
  }

  bs.WriteCompressed((uint16_t)0);  // player x-axis
  bs.WriteCompressed((uint16_t)0);  // player y-axis
  bs.WriteCompressed((uint16_t)0);  // player z-axis
  uint16_t rot = 0;
  WriteBits(bs, rot, 9);

  bs.WriteCompressed((uint32_t)0);  // safezone?
  bs.WriteCompressed((uint32_t)0);  // group ID

  // Unknown
  bs.WriteCompressed((uint32_t)0);
  bs.WriteCompressed((uint32_t)0);
  bs.WriteCompressed((uint8_t)5);
  bs.WriteCompressed((uint8_t)5);
  bs.WriteCompressed((uint8_t)5);
  bs.WriteCompressed((uint8_t)5);
  bs.WriteCompressed((uint8_t)5);
  bs.WriteCompressed((uint8_t)5);
  bs.WriteCompressed((uint8_t)5);
  bs.WriteCompressed((uint8_t)5);

  bs.Write1();  // CC Setting?

  // Tax Settings
  bs.WriteCompressed((uint8_t)0);  // Tax Own
  bs.WriteCompressed((uint8_t)0);  // Tax Ally
  bs.WriteCompressed((uint8_t)0);  // Tax Eco Ally
  bs.WriteCompressed((uint8_t)0);  // Tax Neutral
  bs.WriteCompressed((uint8_t)0);  // Tax Eco Enemy
  bs.WriteCompressed((uint8_t)0);  // Tax Enemy
};

}  // namespace FOMNetwork
