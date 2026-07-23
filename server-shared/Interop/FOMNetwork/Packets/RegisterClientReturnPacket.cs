using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Structs;
using FOMServer.Shared.Interop.FOMNetwork.Structs.Faction;
using FOMServer.Shared.Interop.FOMNetwork.Structs.Item;
using FOMServer.Shared.Interop.FOMNetwork.Structs.Player;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_REGISTER_CLIENT_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RegisterClientReturnPacket
    {
        public const int MaxAvatarCache = 300; // MAX_AVATAR_CACHE

        public byte WorldId;
        public uint PlayerId;
        public StatusCode Status;
        public ItemListInterop Inventory;
        public EquipmentArray Equipment;
        public WeaponsArray Weapons;
        public ActiveConsumablesArray ActiveConsumables;
        public NanomachineAugmentationsArray NanomachineAugmentations;
        public ItemListInterop Storage;
        public QuickSlotsArray Quickslots;
        public AvatarInterop Avatar;
        public PlayerAttributesInterop Attributes;
        public PlayerProfileInterop Profile;
        public byte Unknown1;
        public byte Unknown2;
        public ushort AvatarCacheCount;
        public AvatarCacheArray AvatarCache;
        public byte Unknown3;
        public PositionRotationInterop SafezoneCenter;
        public uint SafezoneRadius;
        public uint NodeId;
        public byte Unknown4;
        public ushort CloningDuration;
        public FactionEmblemInterop FactionEmblem;
        public fixed byte RawFactionName[BufferSizes.FactionName];
        public PlayerSkillsInterop Skills;
        public PositionRotationInterop SpawnPosition;
        public byte SpawnAtPosition;
        public FactionPerksInterop FactionPerks;

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

        [InlineArray(PlayerConstants.NumEquipmentSlots)]
        public struct EquipmentArray
        {
            private ItemInterop _element;
        }

        [InlineArray(PlayerConstants.NumWeaponSlots)]
        public struct WeaponsArray
        {
            private ItemInterop _element;
        }

        [InlineArray(PlayerConstants.NumActiveConsumableSlots)]
        public struct ActiveConsumablesArray
        {
            private ItemInterop _element;
        }

        [InlineArray(PlayerConstants.NumNanomachineAugmentationSlots)]
        public struct NanomachineAugmentationsArray
        {
            private ItemInterop _element;
        }

        [InlineArray(PlayerConstants.NumQuickslots)]
        public struct QuickSlotsArray
        {
            private ItemType _element;
        }

        [InlineArray(MaxAvatarCache)]
        public struct AvatarCacheArray
        {
            private AvatarInterop _element;
        }
    }
}
