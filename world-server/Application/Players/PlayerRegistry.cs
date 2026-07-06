using System.Collections.Concurrent;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.World.Application.Players
{
    internal class PlayerRegistry : IPlayerRegistry
    {
        private readonly IPersistenceService _persistenceService;
        private readonly TimeProvider _timeProvider;
        private readonly IPlayerUpdateService _playerUpdateService;
        private readonly ConcurrentDictionary<uint, Player> _players = new();
        private readonly ConcurrentDictionary<NetworkAddress, Player> _playersByAddress = new();
        private readonly ConcurrentDictionary<uint, PendingPlayer> _pendingPlayers = new();

        public PlayerRegistry(
            IPersistenceService persistenceService,
            TimeProvider timeProvider,
            IPlayerUpdateService playerUpdateService)
        {
            _persistenceService = persistenceService;
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
            var player = new Player(playerId);
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

            if (!_players.TryAdd(playerId, player))
            {
                return null;
            }

            _playersByAddress[sender] = player;
            _persistenceService.Register(player);
            _playerUpdateService.RegisterRecipient(player);
            return player;
        }

        public void Logout(Player player)
        {
            _playerUpdateService.UnregisterRecipient(player);

            _persistenceService.WaitForPersistence(
                player,
                () =>
                {
                    _playersByAddress.TryRemove(new(player.Address, player));
                    _players.TryRemove(new(player.Id, player));
                });
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
