using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;
using FOMServer.World.Core.Players.Registration;
using FOMServer.World.Core.Tick;

namespace FOMServer.World.Application.Players.Registration
{
    internal class PlayerRegistration : IPlayerRegistration
    {
        private readonly Player _player;
        private readonly IPersistenceService _persistenceService;
        private readonly IPlayerEventPacketDispatcher _eventPacketDispatcher;
        private readonly IPlayerUpdateTick _playerUpdateService;

        public PlayerRegistration(
            Player player,
            IPersistenceService persistenceService,
            IPlayerEventPacketDispatcher eventPacketDispatcher,
            IPlayerUpdateTick playerUpdateService
        )
        {
            _player = player;
            _persistenceService = persistenceService;
            _eventPacketDispatcher = eventPacketDispatcher;
            _playerUpdateService = playerUpdateService;
        }

        public void Register()
        {
            foreach (var container in _player.Inventory.GetItemContainers())
            {
                container.ItemsAdded += OnItemsAddedToPlayer;
                container.ItemsRemoved += OnItemsRemovedFromPlayer;
                container.ItemDestroyed += OnPlayerItemDestroyed;
            }

            var persistables = new List<IPersistable>();
            _player.CollectPersistables(persistables);
            foreach (var persistable in persistables)
            {
                _persistenceService.Register(persistable);
            }

            _eventPacketDispatcher.Register(_player);
            _playerUpdateService.Register(_player);
        }

        public void Unregister()
        {
            _playerUpdateService.Unregister(_player);
            _eventPacketDispatcher.Unregister(_player);

            var persistables = new List<IPersistable>();
            _player.CollectPersistables(persistables);
            foreach (var persistable in persistables)
            {
                _persistenceService.Unregister(persistable);
            }

            foreach (var container in _player.Inventory.GetItemContainers())
            {
                container.ItemsAdded -= OnItemsAddedToPlayer;
                container.ItemsRemoved -= OnItemsRemovedFromPlayer;
                container.ItemDestroyed -= OnPlayerItemDestroyed;
            }
        }

        private void OnItemsAddedToPlayer(ItemContainer container, IReadOnlyCollection<Item> items)
        {
            foreach (var persistable in items)
            {
                _persistenceService.Register(persistable);
            }
        }

        private void OnItemsRemovedFromPlayer(ItemContainer container, IReadOnlyCollection<Item> items)
        {
            foreach (var persistable in items)
            {
                _persistenceService.Unregister(persistable);
            }
        }

        private void OnPlayerItemDestroyed(ItemContainer container, Item item)
        {
            _persistenceService.Unregister(item);
        }
    }
}
