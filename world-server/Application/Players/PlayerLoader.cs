using System.Xml.Linq;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Core.Repositories;
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
                items[ItemLocation.Inventory],
                items[ItemLocation.Equipment],
                items[ItemLocation.Weapons],
                items[ItemLocation.ActiveConsumable],
                items[ItemLocation.NanomachineAugmentation]
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

        private Dictionary<ItemLocation, IDictionary<uint, Item>> LoadItems(uint id)
        {
            var loadedItems = new Dictionary<ItemLocation, IDictionary<uint, Item>>
            {
                [ItemLocation.Inventory] = new Dictionary<uint, Item>(),
                [ItemLocation.Equipment] = new Dictionary<uint, Item>(),
                [ItemLocation.Weapons] = new Dictionary<uint, Item>(),
                [ItemLocation.ActiveConsumable] = new Dictionary<uint, Item>(),
                [ItemLocation.NanomachineAugmentation] = new Dictionary<uint, Item>()
            };

            var nextItemId = id * 1000;
            void addItem(ItemType type, ItemLocation location, uint locationId)
            {
                var item = new Item(nextItemId++, type, id, location, locationId, 100, 1000, 1000, 100);
                loadedItems[location][item.Id] = item;

                _persistenceService.Register(item);
            }

            addItem(ItemType._9mmStandardRounds, ItemLocation.Inventory, 0);

            addItem(ItemType.Fedora, ItemLocation.Equipment, (uint)EquipmentSlot.Hat);
            addItem(ItemType.ShieldAugmentation, ItemLocation.Equipment, (uint)EquipmentSlot.Back);
            addItem(ItemType.AlmDesignsGlassesBlack, ItemLocation.Equipment, (uint)EquipmentSlot.Eyes);

            addItem(ItemType.DOA187, ItemLocation.Weapons, 0);

            addItem(ItemType.DoublecheeseMystique, ItemLocation.ActiveConsumable, 0);

            addItem(ItemType.ElectromyographicRegulator, ItemLocation.NanomachineAugmentation, 0);

            return loadedItems;
        }

        private void BindToPlayer(Player player, Dictionary<ItemLocation, IDictionary<uint, Item>> items)
        {
            foreach (var (_, itemList) in items)
            {
                foreach (var (_, item) in itemList)
                {
                    item.BindOwner(player);
                }
            }
        }
    }
}
