using System;
using System.Collections.Generic;
using System.Text;
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
                var items = container.GetAll();
                foreach (var item in items)
                {
                    _persistenceService.Register(item);
                }
            }
            _persistenceService.Register(_player);
            _persistenceService.Register(_player.Attributes);
            _persistenceService.Register(_player.Quickslots);

            _eventPacketDispatcher.Register(_player);
            _playerUpdateService.Register(_player);
        }

        public void Unregister()
        {
            _playerUpdateService.Unregister(_player);
            _eventPacketDispatcher.Unregister(_player);

            _persistenceService.Unregister(_player.Quickslots);
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
