using System.Xml.Linq;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Core.Items;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    internal class PlayerLoader : IPlayerLoader
    {
        private readonly IPersistenceService _persistenceService;
        private readonly IPlayerRepository _playerRepository;

        public PlayerLoader(IPersistenceService persistenceService, IPlayerRepository playerRepository)
        {
            _persistenceService = persistenceService;
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

            var player = new Player(
                id,
                attributes,
                items[ItemContainerType.Inventory]
            );

            BindToPlayer(player, items);

            _persistenceService.Register(player);

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

        private Dictionary<ItemContainerType, IDictionary<uint, Item>> LoadItems(uint id)
        {
            var loadedItems = new Dictionary<ItemContainerType, IDictionary<uint, Item>>
            {
                [ItemContainerType.Inventory] = new Dictionary<uint, Item>(),
            };

            var nextItemId = id * 1000;
            void addItem(ItemContainerType container, ItemType type, ItemSlotType slot = ItemSlotType.None)
            {
                var item = new Item(nextItemId++, type, ItemLocationType.Player, id, slot, 100, 1000, 1000, 100);
                loadedItems[container][item.Id] = item;

                _persistenceService.Register(item);
            }

            addItem(ItemContainerType.Inventory, ItemType._9mmStandardRounds);

            /*
             * addItem(ItemType._9mmStandardRounds, ItemContainerType.Inventory, 0);
            addItem(ItemType.EmergencyMedikit, ItemContainerType.Inventory, 0);
            addItem(ItemType.ShieldAugmentation, ItemContainerType.Inventory, 0);
            addItem(ItemType.BackerTShirtMale, ItemContainerType.Inventory, 0);
            addItem(ItemType.AssaultPantsMale, ItemContainerType.Inventory, 0);
            addItem(ItemType.EsporteAllTerrainShoesMale, ItemContainerType.Inventory, 0);

            addItem(ItemType.Fedora, ItemContainerType.Equipment, (uint)EquipmentSlot.Hat);
            addItem(ItemType.AdvancedCivilianHelmet, ItemContainerType.Equipment, (uint)EquipmentSlot.Head);
            addItem(ItemType.ShieldAugmentation, ItemContainerType.Equipment, (uint)EquipmentSlot.Back);
            addItem(ItemType.AlmDesignsGlassesBlack, ItemContainerType.Equipment, (uint)EquipmentSlot.Eyes);
            addItem(ItemType.AllWeatherTShirtMale, ItemContainerType.Equipment, (uint)EquipmentSlot.Shirt);
            addItem(ItemType.AntiRiotPantsMale, ItemContainerType.Equipment, (uint)EquipmentSlot.Pants);
            addItem(ItemType.BlackDressShoesMale, ItemContainerType.Equipment, (uint)EquipmentSlot.Shoes);

            addItem(ItemType.DOA187, ItemContainerType.Weapons, 0);

            addItem(ItemType.DoublecheeseMystique, ItemContainerType.ActiveConsumable, 0);

            addItem(ItemType.ElectromyographicRegulator, ItemContainerType.NanomachineAugmentation, 0);*/

            return loadedItems;
        }

        private void BindToPlayer(Player player, Dictionary<ItemContainerType, IDictionary<uint, Item>> items)
        {
            foreach (var (_, itemList) in items)
            {
                foreach (var (_, item) in itemList)
                {
                    item.BindLocation(player);
                }
            }
        }
    }
}
