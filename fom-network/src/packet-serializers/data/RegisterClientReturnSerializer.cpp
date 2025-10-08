#include <fom-network/packets/PacketSerializers.h>

#include "../models/AvatarModelSerializer.h"

namespace FOMNetwork {

void RegisterClientReturnSerializer::WriteData(
    RakNet::BitStream& bs, const Packet::RegisterClientReturn& data) const {
  AvatarModelSerializer avatarSerializer;

  bs.WriteCompressed(data.worldID);
  bs.WriteCompressed(data.playerID);
  bs.WriteCompressed(data.status);

  bs.WriteCompressed((uint16_t)0);  // Inventory Stack Count
  // foreach (var group in ThePlayer.Inventory.GroupedItems) {
  //   bs.WriteUShortCompressed(group.Type, Endian.Big);        // item type
  //   bs.WriteUShortCompressed(group.Value, Endian.Big);       // value
  //   bs.WriteUShortCompressed(group.Durability, Endian.Big);  // durability
  //   bs.WriteBit(group.FactionItem);  // faction pool item
  //   bs.WriteUShortCompressed((ushort)group.Items.Count,
  //                            Endian.Big);  // quantity of item

  //  foreach (var item in group.Items) {
  //    bs.WriteUInt32Compressed(item.ItemID, Endian.Big);  // item ID
  //  }
  //}

  for (int i = 0; i < NUM_EQUIPMENT_SLOTS; ++i) {
    bs.Write0();
    // bs.WriteBit(true);                                      // slot equipped
    // bs.WriteUInt32Compressed(item.ItemID, Endian.Big);      // item ID
    // bs.WriteUShortCompressed(item.Type, Endian.Big);        // item type
    // bs.WriteUShortCompressed(item.Value, Endian.Big);       // value
    // bs.WriteUShortCompressed(item.Durability, Endian.Big);  // durability
    // bs.WriteBit(item.FactionItem);                          // faction pool
    // item
  }

  for (int i = 0; i < NUM_WEAPON_SLOTS; ++i) {
    bs.Write0();
    // bs.WriteBit(true);                                      // slot equipped
    // bs.WriteUInt32Compressed(item.ItemID, Endian.Big);      // item ID
    // bs.WriteUShortCompressed(item.Type, Endian.Big);        // item type
    // bs.WriteUShortCompressed(item.Value, Endian.Big);       // value
    // bs.WriteUShortCompressed(item.Durability, Endian.Big);  // durability
    // bs.WriteBit(item.FactionItem);                          // faction pool
    // item
  }

  for (int i = 0; i < NUM_QUICK_SLOTS; ++i) {
    bs.WriteCompressed((uint16_t)0);  // item type
  }

  avatarSerializer.Write(bs, data.avatar);
  bs.Write0(); // Hide Armor

  uint8_t rank = 1;
  WriteBits(bs, rank, 3);

  // Unknown Bits
  bs.Write1();
  bs.Write1();
  bs.Write1();
  bs.Write1();
  bs.Write1();
  bs.Write1();

  bs.WriteCompressed(data.health);
  bs.WriteCompressed(data.stamina);
  bs.WriteCompressed(data.bioEnergy);
  bs.WriteCompressed(data.aura);
  bs.WriteCompressed(data.credits);
  bs.WriteCompressed(data.factionCredits);
  bs.WriteCompressed(data.experiencePoints);
  bs.WriteCompressed(data.penaltyPoints);
  bs.WriteCompressed(data.availableClones);

  bs.WriteCompressed((uint32_t)0);  // Is Prisoner
  bs.WriteCompressed((uint32_t)1);  // Has Faction Privileges

  // Booster Slots
  // ((uint)(canonical.Booster1.ItemType + (canonical.Booster1.EffectDuration <<
  // 16))
  bs.WriteCompressed((uint32_t)0);
  bs.WriteCompressed((uint32_t)0);
  bs.WriteCompressed((uint32_t)0);

  bs.WriteCompressed((uint32_t)0);  // Is Senator
  bs.WriteCompressed((uint32_t)0);  // Is Most Wanted
  bs.WriteCompressed((uint32_t)0);  // Is Fugitive

  // Player Stats
  bs.WriteCompressed((uint32_t)1000);  // Agility
  bs.WriteCompressed((uint32_t)0);     // Ballistic Damage
  bs.WriteCompressed((uint32_t)0);     // Energy Damage
  bs.WriteCompressed((uint32_t)0);     // Stamina Damage
  bs.WriteCompressed((uint32_t)0);     // Bio Damage
  bs.WriteCompressed((uint32_t)0);     // Aura Damage
  bs.WriteCompressed((uint32_t)0);     // Destruction
  bs.WriteCompressed((uint32_t)0);     // Weapon Recoil
  bs.WriteCompressed((uint32_t)0);     // Armor
  bs.WriteCompressed((uint32_t)0);     // Protection Reduction
  bs.WriteCompressed((uint32_t)0);     // Shielding
  bs.WriteCompressed((uint32_t)0);     // Endurance
  bs.WriteCompressed((uint32_t)0);     // Resistance
  bs.WriteCompressed((uint32_t)0);     // Reflection
  bs.WriteCompressed((uint32_t)0);     // Defense Rating
  bs.WriteCompressed((uint32_t)0);     // Block Rating
  bs.WriteCompressed((uint32_t)0);     // Crit Rating
  bs.WriteCompressed((uint32_t)0);     // Health Regen
  bs.WriteCompressed((uint32_t)0);     // Stamina Regen
  bs.WriteCompressed((uint32_t)0);     // BioEnergy Regen
  bs.WriteCompressed((uint32_t)0);     // Aura Regen
  bs.WriteCompressed((uint32_t)0);     // Addiction
  bs.WriteCompressed((uint32_t)0);     // Addiction Treatment
  bs.WriteCompressed((uint32_t)0);     // Coins
  bs.WriteCompressed((uint32_t)0);     // Drug Cooldown
  bs.WriteCompressed((uint32_t)0);     // Food Cooldown
  bs.WriteCompressed((uint32_t)0);     // Xeno Damage
  bs.WriteCompressed((uint32_t)0);     // Health Drain
  bs.WriteCompressed((uint32_t)0);     // Stamina Drain
  bs.WriteCompressed((uint32_t)0);     // BioEnergy Drain
  bs.WriteCompressed((uint32_t)0);     // Aura Loss
  bs.WriteCompressed((uint32_t)0);     // Medikit Cooldown

  // Unknown
  bs.WriteCompressed((uint32_t)0);
  bs.Write1();
  bs.Write1();

  EncodeString(bs, data.name);
  bs.WriteCompressed((uint32_t)0);  // Department Name, Null Terminator (No name)

  // Unknown String (RegisterClient3?)
  bs.WriteCompressed(
      (uint32_t)0);  // sc.EncodeString("RegisterClient3", 2048, bs, 0);

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
    bs.WriteCompressed(
        (uint32_t)0);
    bs.WriteCompressed(
        (uint16_t)0);  // Item Type
    bs.WriteCompressed((uint8_t)0);  // Cooling %
    bs.WriteCompressed((uint8_t)0);  // Heating %
    bs.WriteCompressed((uint8_t)0);
    bs.WriteCompressed((uint8_t)0);
    bs.Write0();
    bs.WriteCompressed((uint8_t)0);  // quantity of units completed
    bs.WriteCompressed((uint8_t)0);  // quantity of units queued
    bs.Write0();      // Paused
    bs.WriteCompressed((uint8_t)0);
    bs.WriteCompressed((uint32_t)0);  // Base Cost Per Unit
    bs.WriteCompressed((uint8_t)0);  // Tax Rate
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
