using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

namespace FOMServer.Shared.Tests.Factories
{
    public class TestItemBuilder
    {
        private static uint s_nextItemId = 1;

        private readonly uint _id;
        private readonly ItemType _itemType;

        private ItemLocationType _locationType;
        private uint _locationId;
        private ItemSlotType _slot;
        private ushort _value;
        private ushort _durability;
        private ushort _valueMax;
        private byte _durabilityLossFactor;
        private ItemSecurity _security;
        private uint _creatorPlayerId;
        private uint _stolenFromPlayerId;
        private uint _timeout;
        private byte _recipeVariation;
        private ItemRarity _rarity;
        private byte _attributeBonus;
        private readonly byte[] _recipeBalanceValues = new byte[BufferSizes.NumItemBalanceSliders];

        public TestItemBuilder(uint id, ItemType type)
        {
            _id = id;
            _itemType = type;

            _locationType = ItemLocationType.None;
            _locationId = 0;
            _slot = ItemSlotType.None;
            _value = 0;
            _durability = 10000;
            _valueMax = 10000;
            _durabilityLossFactor = 100;
            _security = ItemSecurity.Normal;
            _creatorPlayerId = 0;
            _stolenFromPlayerId = 0;
            _timeout = 0;
            _recipeVariation = 0;
            _rarity = ItemRarity.Standard;
            _attributeBonus = 0;
            for (var i = 0; i < _recipeBalanceValues.Length; ++i)
            {
                _recipeBalanceValues[i] = 50;
            }
        }

        public TestItemBuilder WithLocation(ItemLocationType type, uint id)
        {
            _locationType = type;
            _locationId = id;
            return this;
        }

        public TestItemBuilder WithSlot(ItemSlotType slot)
        {
            _slot = slot;
            return this;
        }

        public TestItemBuilder WithValue(ushort value)
        {
            _value = value;
            return this;
        }

        public TestItemBuilder WithDurability(ushort durability)
        {
            _durability = durability;
            return this;
        }

        public TestItemBuilder WithValueMax(ushort valueMax)
        {
            _valueMax = valueMax;
            return this;
        }

        public TestItemBuilder WithDurabilityLossFactor(byte durabilityLossFactor)
        {
            _durabilityLossFactor = durabilityLossFactor;
            return this;
        }

        public TestItemBuilder WithSecurity(ItemSecurity security)
        {
            _security = security;
            return this;
        }

        public TestItemBuilder WithCreatorPlayerId(uint playerId)
        {
            _creatorPlayerId = playerId;
            return this;
        }

        public TestItemBuilder WithStolenFromPlayerId(uint playerId)
        {
            _stolenFromPlayerId = playerId;
            return this;
        }

        public TestItemBuilder WithTimeout(uint timeout)
        {
            _timeout = timeout;
            return this;
        }

        public TestItemBuilder WithRecipeVariation(byte recipeVariation)
        {
            _recipeVariation = recipeVariation;
            return this;
        }

        public TestItemBuilder WithRarity(ItemRarity rarity)
        {
            _rarity = rarity;
            return this;
        }

        public TestItemBuilder WithAttributeBonus(byte bonus)
        {
            _attributeBonus = bonus;
            return this;
        }

        public TestItemBuilder WithRecipeBalanceValue(int index, byte value)
        {
            if (index >= BufferSizes.NumItemBalanceSliders)
            {
                throw new ArgumentException(
                    nameof(index),
                    $"Index {index} must be below {BufferSizes.NumItemBalanceSliders}"
                );
            }

            _recipeBalanceValues[index] = value;
            return this;
        }

        public Item Build()
        {
            return new Item(
                _id,
                _itemType,
                _locationType,
                _locationId,
                _slot,
                _value,
                _durability,
                _valueMax,
                _durabilityLossFactor,
                _security,
                _creatorPlayerId,
                _stolenFromPlayerId,
                _timeout,
                _recipeVariation,
                _rarity,
                _attributeBonus,
                _recipeBalanceValues
            );
        }

        public static TestItemBuilder Create(ItemType type, uint? id = null)
        {
            // Advance the automatic incrementing ID if necessary to avoid duplicates.
            if (id is not null && id > s_nextItemId)
            {
                s_nextItemId = id.Value + 1;
            }

            return new TestItemBuilder(id ?? s_nextItemId++, type);
        }
    }
}
