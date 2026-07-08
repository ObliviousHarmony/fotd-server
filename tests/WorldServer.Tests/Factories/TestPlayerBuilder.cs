using System.Xml.Linq;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Tests.Factories
{
    internal class TestPlayerBuilder
    {
        private readonly uint _id;
        private uint _nextItemId;
        private readonly uint[] _attributes = new uint[(int)AttributeType.NUM_ATTRIBUTE_TYPES];
        private readonly Dictionary<ItemLocation, Dictionary<uint, Item>> _items;

        public TestPlayerBuilder(uint id)
        {
            _id = id;
            _nextItemId = id * 1000;

            for (var i = 0; i < (int)AttributeType.NUM_ATTRIBUTE_TYPES; ++i)
            {
                _attributes[i] = PlayerAttributes.GetMetadata((AttributeType)i).Default;
            }

            _items = new Dictionary<ItemLocation, Dictionary<uint, Item>>
            {
                [ItemLocation.Inventory] = [],
                [ItemLocation.Equipment] = [],
                [ItemLocation.Weapons] = [],
                [ItemLocation.ActiveConsumable] = [],
                [ItemLocation.NanomachineAugmentation] = []
            };
        }

        public TestPlayerBuilder WithAttribute(AttributeType type, uint value)
        {
            _attributes[(uint)type] = value;
            return this;
        }

        public TestPlayerBuilder WithInventory(AttributeType type, uint value)
        {
            _attributes[(uint)type] = value;

            return this;
        }

        public TestPlayerBuilder WithItem(
            ItemType type,
            ItemLocation location,
            uint locationId,
            ushort value,
            ushort durability,
            ushort maxDurability,
            byte durabilityLossFactor)
        {
            if (!_items.TryGetValue(location, out var itemList))
            {
                throw new InvalidOperationException($"Item location {location} is invalid");
            }

            var item = new Item(_nextItemId++, type, _id, location, locationId, value, durability, maxDurability, durabilityLossFactor);
            itemList[item.Id] = item;

            return this;
        }

        public Player Build()
        {
            var player = new Player(
                _id,
                _attributes,
                _items[ItemLocation.Inventory],
                _items[ItemLocation.Equipment],
                _items[ItemLocation.Weapons],
                _items[ItemLocation.ActiveConsumable],
                _items[ItemLocation.NanomachineAugmentation]
            );

            BindToPlayer(player);

            return player;
        }

        public static TestPlayerBuilder Create(uint id)
        {
            return new TestPlayerBuilder(id);
        }

        private void BindToPlayer(Player player)
        {
            foreach (var (_, itemList) in _items)
            {
                foreach (var (_, item) in itemList)
                {
                    item.BindOwner(player);
                }
            }
        }
    }
}
