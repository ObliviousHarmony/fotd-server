using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Types
{
    public static class InventoryConstants
    {
        public const int NumUnknownItemSlots = 6; // NUM_UNKNOWN_ITEM_SLOTS
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Inventory
    {
        public ItemList Items;
        public EquipmentList Equipment;
        public WeaponList Weapons;
        public UnknownItemList Unknown1;
        public ItemList Storage;

        [InlineArray((int)EquipmentSlot.NUM_EQUIPMENT_SLOTS)]
        public struct EquipmentList
        {
            private Item _item;
        }

        [InlineArray((int)WeaponSlot.NUM_WEAPON_SLOTS)]
        public struct WeaponList
        {
            private Item _item;
        }

        [InlineArray(InventoryConstants.NumUnknownItemSlots)]
        public struct UnknownItemList
        {
            private Item _item;
        }
    }
}
