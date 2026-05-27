using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_REGISTER_CLIENT_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RegisterClientReturn
    {
        public byte WorldId;
        public uint PlayerId;
        public StatusCode Status;
        public ItemList Inventory;
        public EquipmentArray Equipment;
        public WeaponsArray Weapons;
        public UnknownSlotsArray UnknownSlots;
        public ItemList Storage;
        public fixed ushort QuickSlots[PlayerConstants.NumQuickSlots];
        public Avatar Avatar;
        public PlayerAttributes Attributes;
        public PlayerProfile Profile;
        public byte Unknown1;
        public byte Unknown2;
        public ushort AvatarCacheCount;
        public AvatarCacheArray AvatarCache;
        public byte Unknown3;
        public Position SafezoneCenter;
        public uint SafezoneRadius;
        public uint NodeId;
        public byte Unknown4;
        public ushort CloningDuration;
        public FactionEmblem FactionEmblem;
        public fixed byte RawFactionName[BufferSizes.FactionName];
        public PlayerSkills Skills;
        public Position SpawnPosition;
        public byte SpawnAtPosition;
        public FactionPerks FactionPerks;

        public enum StatusCode : byte
        {
            Success = 1, // REGISTER_CLIENT_RETURN_SUCCESS
            WorldFull = 4, // REGISTER_CLIENT_RETURN_WORLD_FULL
            InvalidWorldFile = 5, // REGISTER_CLIENT_RETURN_INVALID_WORLD_FILE
        }

        public string FactionName
        {
            set
            {
                fixed (byte* ptr = RawFactionName)
                {
                    CStringParser.FromString(value, ptr, BufferSizes.FactionName);
                }
            }
        }

        [InlineArray((int)EquipmentSlot.NUM_EQUIPMENT_SLOTS)]
        public struct EquipmentArray
        {
            private Item _element;
        }

        [InlineArray(PlayerConstants.NumWeaponSlots)]
        public struct WeaponsArray
        {
            private Item _element;
        }

        [InlineArray(PlayerConstants.NumUnknownItemSlots)]
        public struct UnknownSlotsArray
        {
            private Item _element;
        }

        [InlineArray(PlayerConstants.MaxAvatarCache)]
        public struct AvatarCacheArray
        {
            private Avatar _element;
        }
    }
}
