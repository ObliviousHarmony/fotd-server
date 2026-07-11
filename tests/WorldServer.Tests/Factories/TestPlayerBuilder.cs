using System.Xml.Linq;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Core.Items;
using FOMServer.World.Core.Players;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.World.Tests.Factories
{
    internal class TestPlayerBuilder
    {
        private readonly uint _id;
        private uint _nextItemId;
        private readonly uint[] _attributes = new uint[(int)AttributeType.NUM_ATTRIBUTE_TYPES];
        private readonly Dictionary<ItemContainerType, Dictionary<uint, Item>> _items;
        private readonly ItemType[] _quickslots;

        public TestPlayerBuilder(uint id)
        {
            _id = id;
            _nextItemId = id * 1000;

            for (var i = 0; i < (int)AttributeType.NUM_ATTRIBUTE_TYPES; ++i)
            {
                _attributes[i] = PlayerAttributes.GetMetadata((AttributeType)i).Default;
            }

            _items = new Dictionary<ItemContainerType, Dictionary<uint, Item>>
            {
                [ItemContainerType.Inventory] = [],
            };

            _quickslots = new ItemType[PlayerConstants.NumQuickSlots];
            for (var i = 0; i < _quickslots.Length; ++i)
            {
                _quickslots[i] = ItemType.Invalid;
            }
        }

        public TestPlayerBuilder WithAttribute(AttributeType type, uint value)
        {
            _attributes[(uint)type] = value;
            return this;
        }

        public TestPlayerBuilder WithItem(
            ItemContainerType container,
            ItemType type,
            ItemLocationType locationType,
            uint locationId,
            ItemSlotType slot,
            ushort value,
            ushort durability,
            ushort maxDurability,
            byte durabilityLossFactor)
        {
            if (!_items.TryGetValue(container, out var itemList))
            {
                throw new InvalidOperationException($"Item container {container} is invalid");
            }

            var item = new Item(_nextItemId++, type, locationType, locationId, slot, value, durability, maxDurability, durabilityLossFactor);
            itemList[item.Id] = item;

            return this;
        }

        public TestPlayerBuilder WithQuickslot(ItemSlotType slot, ItemType type)
        {
            if (slot is not ( >= ItemSlotType.QuickslotStart and < ItemSlotType.QuickslotEnd))
            {
                throw new ArgumentException("The provided slot is not a quickslot", nameof(slot));
            }

            _quickslots[slot - ItemSlotType.QuickslotStart] = type;

            return this;
        }

        public Player Build()
        {
            var player = new Player(
                _id,
                _attributes,
                _items[ItemContainerType.Inventory],
                _quickslots
            );

            return player;
        }

        public static TestPlayerBuilder Create(uint id)
        {
            return new TestPlayerBuilder(id);
        }
    }
}
