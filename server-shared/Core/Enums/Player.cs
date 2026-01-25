namespace FOMServer.Shared.Core.Enums
{
    public enum AvatarSex : byte
    {
        Male = 0, // MALE
        Female = 1, // FEMALE
    }

    public enum AvatarRace : byte
    {
        White = 0, // WHITE
        Black = 1, // BLACK
    }

    public enum EquipmentSlot : byte
    {
        // Basic slots (always serialized)
        Shirt = 0, // EQUIPMENT_SLOT_SHIRT
        Bottoms = 1, // EQUIPMENT_SLOT_BOTTOMS
        Shoes = 2, // EQUIPMENT_SLOT_SHOES
        NUM_BASIC_EQUIPMENT_SLOTS = 3,

        // Extended slots (conditionally serialized)
        Hat = 3, // EQUIPMENT_SLOT_HAT
        Head = 4, // EQUIPMENT_SLOT_HEAD
        Eyes = 5, // EQUIPMENT_SLOT_EYES
        Shoulder = 6, // EQUIPMENT_SLOT_SHOULDER
        Arms = 7, // EQUIPMENT_SLOT_ARMS
        Torso = 8, // EQUIPMENT_SLOT_TORSO
        Back = 9, // EQUIPMENT_SLOT_BACK
        Legs = 10, // EQUIPMENT_SLOT_LEGS
        Hands = 11, // EQUIPMENT_SLOT_HANDS

        NUM_EQUIPMENT_SLOTS = 12,
    }
}
