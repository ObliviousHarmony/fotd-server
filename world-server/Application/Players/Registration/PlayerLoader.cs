using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Repositories;
using FOMServer.World.Core.Players;
using FOMServer.World.Core.Players.Registration;

namespace FOMServer.World.Application.Players.Registration
{
    internal class PlayerLoader : IPlayerLoader
    {
        private readonly IPlayerRepository _playerRepository;

        public PlayerLoader(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public Player? Load(uint id)
        {
            var playerDto = _playerRepository.GetById(id);
            if (playerDto is null)
            {
                return null;
            }

            var attributes = LoadAttributes(id);
            var items = LoadItems(id);
            var quickslots = LoadQuickslots(id);

            var player = new Player(id, attributes, items, quickslots);

            return player;
        }

        private uint[] LoadAttributes(uint id)
        {
            var attributes = new uint[(int)AttributeType.NUM_ATTRIBUTE_TYPES];
            for (var i = 0; i < (int)AttributeType.NUM_ATTRIBUTE_TYPES; ++i)
            {
                attributes[i] = PlayerAttributes.GetMetadata((AttributeType)i).Default;
            }

            attributes[(int)AttributeType.Stamina] = 10000;

            return attributes;
        }

        private IDictionary<uint, Item> LoadItems(uint id)
        {
            Dictionary<uint, Item> loadedItems = [];

            var nextItemId = id * 1000;
            void addItem(ItemType type, ItemSlotType slot = ItemSlotType.None)
            {
                var item = new Item(nextItemId++, type, ItemLocationType.Inventory, id, slot, 100, 1000, 1000, 100);
                loadedItems[item.Id] = item;
            }

            addItem(ItemType.Techtronic6x6);
            addItem(ItemType._9mmStandardRounds);
            addItem(ItemType.BackerHelmet);

            addItem(ItemType._9mmStandardRounds);
            addItem(ItemType.EmergencyMedikit);
            addItem(ItemType.ShieldAugmentation);
            addItem(ItemType.BackerTShirtMale);
            addItem(ItemType.AssaultPantsMale);
            addItem(ItemType.EsporteAllTerrainShoesMale);

            addItem(ItemType.Fedora, ItemSlotType.Hat);
            addItem(ItemType.AdvancedCivilianHelmet, ItemSlotType.Head);
            addItem(ItemType.ShieldAugmentation, ItemSlotType.Back);
            addItem(ItemType.AlmDesignsGlassesBlack, ItemSlotType.Eyes);
            addItem(ItemType.AllWeatherTShirtMale, ItemSlotType.Shirt);
            addItem(ItemType.AntiRiotPantsMale, ItemSlotType.Pants);
            addItem(ItemType.BlackDressShoesMale, ItemSlotType.Shoes);

            addItem(ItemType.DOA187, ItemSlotType.Weapon1);

            return loadedItems;
        }

        private ItemType[] LoadQuickslots(uint id)
        {
            var quickslots = new ItemType[PlayerConstants.NumQuickslots];
            for (var i = 0; i < quickslots.Length; ++i)
            {
                quickslots[i] = ItemType.Invalid;
            }

            quickslots[1] = ItemType.AdrenalineAutoinjector;

            return quickslots;
        }
    }
}
