namespace FOMServer.Shared.Interop.FOMNetwork.Enums.Item
{
    public enum ItemSlotType : byte
    {
        None = 0, // ITEM_SLOT_NONE

        WeaponStart = 1, // WEAPON_SLOT_START
        Weapon1 = WeaponStart, // ITEM_SLOT_WEAPON_1
        Weapon2 = 2, // ITEM_SLOT_WEAPON_1
        Weapon3 = 3, // ITEM_SLOT_WEAPON_1
        WeaponEnd = 4, // WEAPON_SLOT_END
        NumWeaponSlots = WeaponEnd - WeaponStart, // NUM_WEAPON_SLOTS

        EquipmentStart = 5, // EQUIPMENT_SLOT_START
        Head = EquipmentStart, // ITEM_SLOT_HEAD
        Eyes = 6, // ITEM_SLOT_EYES
        Shoulders = 7, // ITEM_SLOT_SHOULDERS
        Torso = 8, // ITEM_SLOT_TORSO
        Arms = 9, // ITEM_SLOT_ARMS
        Hands = 10, // ITEM_SLOT_HANDS
        Legs = 11, // ITEM_SLOT_LEGS
        Back = 12, // ITEM_SLOT_BACK
        Hat = 13, // ITEM_SLOT_HAT
        Shirt = 14, // ITEM_SLOT_SHIRT
        Pants = 15, // ITEM_SLOT_PANTS
        Shoes = 16, // ITEM_SLOT_SHOES
        EquipmentEnd = 17, // EQUIPMENT_SLOT_END
        NumEquipmentSlots = EquipmentEnd - EquipmentStart, // NUM_EQUIPMENT_SLOTS

        QuickslotStart = 18, // QUICKSLOT_START
        Quickslot1 = QuickslotStart, // ITEM_SLOT_QUICKSLOT_1
        Quickslot2 = 19, // ITEM_SLOT_QUICKSLOT_2
        Quickslot3 = 20, // ITEM_SLOT_QUICKSLOT_3
        Quickslot4 = 21, // ITEM_SLOT_QUICKSLOT_4
        QuickslotEnd = 22, // QUICKSLOT_END
        NumQuickslots = QuickslotEnd - QuickslotStart, // NUM_QUICKSLOTS

        NanoAug = 26, // ITEM_SLOT_NANO_AUG

        MurderCard = 52, // ITEM_SLOT_MURDER_CARD
    }
}
