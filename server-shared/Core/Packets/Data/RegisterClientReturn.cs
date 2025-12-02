using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Models;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_REGISTER_CLIENT_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RegisterClientReturn
    {
        public const int NameSize = 20;

        public WorldID WorldID;
        public uint PlayerID;
        public StatusCode Status;
        public ushort NumInventoryItems;
        public InventoryBuffer InventoryItems;
        public EquipmentSlots Equipment;
        public WeaponSlots Weapons;
        public fixed ushort QuickSlots[PlayerConstants.NUM_QUICK_SLOTS]; // ItemType Enum
        public AvatarModel Avatar;
        public PlayerAttributesModel Attributes;
        public fixed byte RawName[NameSize];
        public byte SelectedNode;

        public enum StatusCode : byte
        {
            REGISTER_CLIENT_RETURN_INVALID = 0,
            REGISTER_CLIENT_RETURN_SUCCESS = 1,
            REGISTER_CLIENT_RETURN_ERROR = 2,
            REGISTER_CLIENT_RETURN_WORLD_FULL = 4,
            REGISTER_CLIENT_RETURN_INTEGRITY_CHECK_FAILED = 5,
        };

        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                    return CStringParser.ToString(ptr, NameSize);
            }
            set
            {
                fixed (byte* ptr = RawName)
                    CStringParser.FromString(value, ptr, NameSize);
            }
        }

        [InlineArray(PlayerConstants.MaxInventoryItems)]
        public struct InventoryBuffer
        {
            public ItemModel Item;
        }

        [InlineArray((int)EquipmentSlot.NUM_EQUIPMENT_SLOTS)]
        public struct EquipmentSlots
        {
            public ItemSlotModel Slot;
        }

        [InlineArray(PlayerConstants.NUM_WEAPON_SLOTS)]
        public struct WeaponSlots
        {
            public ItemSlotModel Slot;
        }
    }
}
