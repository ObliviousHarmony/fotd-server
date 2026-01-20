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
        Hat = 0, // HAT
        Head = 1, // HEAD
        Eyes = 2, // EYES
        Shoulder = 3, // SHOULDER
        Arms = 4, // ARMS
        Torso = 5, // TORSO
        Back = 6, // BACK
        Legs = 7, // LEGS
        Hands = 8, // HANDS

        NUM_EQUIPMENT_SLOTS = 9
    }
}
