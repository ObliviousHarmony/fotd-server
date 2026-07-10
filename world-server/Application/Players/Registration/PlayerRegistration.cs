using System;
using System.Collections.Generic;
using System.Text;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;
using FOMServer.World.Core.Players.Registration;
using FOMServer.World.Core.Tick;

namespace FOMServer.World.Application.Players.Registration
{
    internal class PlayerRegistration
    {
        private readonly Player _player;
        private readonly IPersistenceService _persistenceService;
        private readonly IPlayerUpdateTick _playerUpdateService;
        private readonly IItemPacketDispatcher _itemPacketDispatcher;

        public PlayerRegistration(
            Player player,
            IPersistenceService persistenceService,
            IPlayerUpdateTick playerUpdateService,
            IItemPacketDispatcher itemPacketDispatcher
        )
        {
            _player = player;
            _persistenceService = persistenceService;
            _playerUpdateService = playerUpdateService;
            _itemPacketDispatcher = itemPacketDispatcher;
        }

        public void Register()
        {
            foreach (var container in _player.Inventory.GetItemContainers())
            {
                var items = container.GetAll();
                foreach (var item in items)
                {
                    _persistenceService.Register(item);
                }
            }
            _persistenceService.Register(_player);
            _persistenceService.Register(_player.Attributes);

            _itemPacketDispatcher.Register(_player.Inventory);

            _playerUpdateService.Register(_player);
        }

        public void Unregister()
        {
            _playerUpdateService.Unregister(_player);

            _itemPacketDispatcher.Unregister(_player.Inventory);

            _persistenceService.Unregister(_player.Attributes);
            _persistenceService.Unregister(_player);
            foreach (var container in _player.Inventory.GetItemContainers())
            {
                var items = container.GetAll();
                foreach (var item in items)
                {
                    _persistenceService.Unregister(item);
                }
            }
        }
    }
}
