namespace FOMServer.Shared.Core.Enums
{
    public enum ItemSlot : byte
    {
        Inventory = 0, // ITEM_SLOT_INVENTORY
        Weapon = 1, // ITEM_SLOT_WEAPON

        EquipmentStart = 5, // ITEM_SLOT_EQUIPMENT_START
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
        EquipmentEnd = 17, // ITEM_SLOT_EQUIPMENT_END

        NanoAug = 26, // ITEM_SLOT_NANO_AUG

        MurderCard = 52, // ITEM_SLOT_MURDER_CARD
    }
}
