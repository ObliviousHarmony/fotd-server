using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Structs;

namespace FOMServer.World.Core.Players
{
    internal delegate void PlayerAvatarChangedHandler(PlayerAvatar avatar);

    internal class PlayerAvatar
    {
        private readonly Lock _syncRoot = new();

        private readonly Player _player;

        private readonly AvatarConstants.Sex _sex;
        private readonly AvatarConstants.Race _race;
        private readonly ushort _face;
        private readonly ushort _hair;
        private readonly ItemType[] _equipmentCache;

        public PlayerAvatar(
            Player player,
            AvatarConstants.Sex sex,
            AvatarConstants.Race race,
            ushort face,
            ushort hair,
            IReadOnlyCollection<ItemSnapshot> equipment
        )
        {
            _player = player;
            _sex = sex;
            _race = race;
            _face = face;
            _hair = hair;

            _equipmentCache = new ItemType[(int)ItemSlotType.NumEquipmentSlots];
            for (var i = ItemSlotType.EquipmentStart; i < ItemSlotType.EquipmentEnd; ++i)
            {
                SetEquipment(i, ItemType.Invalid);
            }
            foreach (var item in equipment)
            {
                SetEquipment(item.Slot, item.Type);
            }
        }

        public event PlayerAvatarChangedHandler? AvatarChanged;

        public uint PlayerId => _player.Id;

        public void UpdateEquipment(IReadOnlyCollection<ItemSnapshot> equipment)
        {
            lock (_syncRoot)
            {
                for (var i = ItemSlotType.EquipmentStart; i < ItemSlotType.EquipmentEnd; ++i)
                {
                    SetEquipment(i, ItemType.Invalid);
                }

                foreach (var item in equipment)
                {
                    SetEquipment(item.Slot, item.Type);
                }
            }

            AvatarChanged?.Invoke(this);
        }

        public void WriteTo(ref AvatarInterop avatar)
        {
            lock (_syncRoot)
            {
                avatar.Sex = _sex;
                avatar.Race = _race;
                avatar.Face = _face;
                avatar.Hair = _hair;

                avatar.FactionId = 0;
                avatar.RankId = 0;
                avatar.Unknown1 = 0;
                avatar.LegacyFactionId = 0;

                avatar.Shirt = GetEquipment(ItemSlotType.Shirt);
                avatar.Bottoms = GetEquipment(ItemSlotType.Bottoms);
                avatar.Shoes = GetEquipment(ItemSlotType.Shoes);
                avatar.Head = GetEquipment(ItemSlotType.Head);
                avatar.Hat = GetEquipment(ItemSlotType.Hat);
                avatar.Eyes = GetEquipment(ItemSlotType.Eyes);
                avatar.Shoulders = GetEquipment(ItemSlotType.Shoulders);
                avatar.Arms = GetEquipment(ItemSlotType.Arms);
                avatar.Torso = GetEquipment(ItemSlotType.Torso);
                avatar.Back = GetEquipment(ItemSlotType.Back);
                avatar.Legs = GetEquipment(ItemSlotType.Legs);
                avatar.Hands = GetEquipment(ItemSlotType.Hands);

                avatar.IsCommander = 0;
                avatar.Unknown2 = 0;
                avatar.Unknown3 = 0;
                avatar.IsGroupLeader = 0;
            }
        }

        private ItemType GetEquipment(ItemSlotType slotType)
        {
            return _equipmentCache[slotType - ItemSlotType.EquipmentStart];
        }

        private void SetEquipment(ItemSlotType slotType, ItemType itemType)
        {
            if (slotType is < ItemSlotType.EquipmentStart or >= ItemSlotType.EquipmentEnd)
            {
                return;
            }

            if (itemType == ItemType.Invalid)
            {
                AvatarConstants.FallbackEquipment[_sex].TryGetValue(slotType, out itemType);
            }

            _equipmentCache[slotType - ItemSlotType.EquipmentStart] = itemType;
        }
    }
}
