using System.Collections.Concurrent;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Application.Players.Registration;
using FOMServer.World.Core.Players;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.World.Application.Players
{
    internal class PlayerRegistry : IPlayerRegistry
    {
        private readonly IPlayerLoader _playerLoader;
        private readonly TimeProvider _timeProvider;
        private readonly IPersistenceService _persistenceService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<uint, Player> _players = new();
        private readonly ConcurrentDictionary<NetworkAddress, Player> _playersByAddress = new();
        private readonly ConcurrentDictionary<uint, PlayerRegistration> _playerRegistrations = new();
        private readonly ConcurrentDictionary<uint, PendingPlayer> _pendingPlayers = new();

        public PlayerRegistry(IPlayerLoader playerLoader, TimeProvider timeProvider, IPersistenceService persistenceService, IServiceProvider serviceProvider)
        {
            _playerLoader = playerLoader;
            _timeProvider = timeProvider;
            _persistenceService = persistenceService;
            _serviceProvider = serviceProvider;
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

            var registration = ActivatorUtilities.CreateInstance<PlayerRegistration>(_serviceProvider, player);

            registration.Register();
            if (!_players.TryAdd(player.Id, player))
            {
                registration.Unregister();
                return null;
            }
            _playersByAddress[player.Address] = player;
            _playerRegistrations[player.Id] = registration;

            return player;
        }

        public void Logout(Player player)
        {
            _persistenceService.WaitForPersistence(
                player,
                () => {
                    if (_playerRegistrations.TryRemove(player.Id, out var registration))
                    {
                        registration.Unregister();
                    }

                    _playersByAddress.TryRemove(new(player.Address, player));
                    _players.TryRemove(new(player.Id, player));
                }
            );
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
