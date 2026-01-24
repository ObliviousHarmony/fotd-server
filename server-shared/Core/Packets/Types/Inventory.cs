using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Inventory
    {
        public ItemList Items;
        public EquipmentList Equipment;
        public WeaponList Weapons;
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
    }
}
