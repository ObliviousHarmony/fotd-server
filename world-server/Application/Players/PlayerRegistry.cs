using System.Collections.Concurrent;
using System.Reflection;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.World.Application.Players
{
    internal class PlayerRegistry : IPlayerRegistry
    {
        private readonly IPlayerLoader _playerLoader;
        private readonly IPersistenceService _persistenceService;
        private readonly IItemPacketDispatcher _itemPacketDispatcher;
        private readonly TimeProvider _timeProvider;
        private readonly IPlayerUpdateService _playerUpdateService;
        private readonly ConcurrentDictionary<uint, Player> _players = new();
        private readonly ConcurrentDictionary<NetworkAddress, Player> _playersByAddress = new();
        private readonly ConcurrentDictionary<uint, PendingPlayer> _pendingPlayers = new();

        public PlayerRegistry(
            IPlayerLoader playerLoader,
            IPersistenceService persistenceService,
            IItemPacketDispatcher itemPacketDispatcher,
            TimeProvider timeProvider,
            IPlayerUpdateService playerUpdateService)
        {
            _playerLoader = playerLoader;
            _persistenceService = persistenceService;
            _itemPacketDispatcher = itemPacketDispatcher;
            _timeProvider = timeProvider;
            _playerUpdateService = playerUpdateService;
        }

        public Player? Get(uint playerId)
        {
            return _players.GetValueOrDefault(playerId);
        }

        public Player? Get(NetworkAddress address)
        {
            return _playersByAddress.GetValueOrDefault(address);
        }

        public IEnumerable<Player> GetAll()
        {
            return _players.Values;
        }

        public Player PrepareForClient(uint playerId, uint clientBinaryAddress)
        {
            var player = _playerLoader.Load(playerId) ?? throw new InvalidOperationException($"Unable to load player {playerId}");

            _pendingPlayers[playerId] = new PendingPlayer(player, clientBinaryAddress, _timeProvider.GetUtcNow());

            return player;
        }

        public Player? ClaimForClient(uint playerId, NetworkAddress sender)
        {
            if (!_pendingPlayers.TryGetValue(playerId, out var pending))
            {
                return null;
            }

            if (pending.IsExpired(_timeProvider.GetUtcNow()))
            {
                _pendingPlayers.TryRemove(new(playerId, pending));
                return null;
            }

            if (sender.BinaryAddress != pending.ExpectedBinaryAddress)
            {
                return null;
            }

            if (!_pendingPlayers.TryRemove(new(playerId, pending)))
            {
                return null;
            }

            var player = pending.Player;
            player.ClaimForClient(sender);

            RegisterPlayer(player);

            return player;
        }

        public void Logout(Player player)
        {
            _persistenceService.WaitForPersistence(
                player,
                () => UnregisterPlayer(player)
            );
        }

        private bool RegisterPlayer(Player player)
        {
            foreach (var container in player.Inventory.GetItemContainers())
            {
                var items = container.GetAll();
                foreach (var item in items)
                {
                    _persistenceService.Register(item);
                }
            }
            _persistenceService.Register(player);
            _persistenceService.Register(player.Attributes);

            _itemPacketDispatcher.Register(player.Inventory);

            _playerUpdateService.RegisterRecipient(player);

            if (!_players.TryAdd(player.Id, player))
            {
                UnregisterPlayer(player);
                return false;
            }
            _playersByAddress[player.Address] = player;

            return true;
        }

        private void UnregisterPlayer(Player player)
        {
            _playersByAddress.TryRemove(new(player.Address, player));
            _players.TryRemove(new(player.Id, player));

            _playerUpdateService.UnregisterRecipient(player);

            _itemPacketDispatcher.Unregister(player.Inventory);

            _persistenceService.Unregister(player.Attributes);
            _persistenceService.Unregister(player);
            foreach (var container in player.Inventory.GetItemContainers())
            {
                var items = container.GetAll();
                foreach (var item in items)
                {
                    _persistenceService.Unregister(item);
                }
            }
        }

        private readonly record struct PendingPlayer
        {
            private static readonly TimeSpan s_lifespan = TimeSpan.FromSeconds(30);

            private readonly DateTimeOffset _deadlineUtc;

            public PendingPlayer(Player player, uint expectedBinaryAddress, DateTimeOffset now)
            {
                Player = player;
                ExpectedBinaryAddress = expectedBinaryAddress;
                _deadlineUtc = now + s_lifespan;
            }

            public Player Player { get; }

            public uint ExpectedBinaryAddress { get; }

            public bool IsExpired(DateTimeOffset now)
            {
                return now >= _deadlineUtc;
            }
        }
    }
}
