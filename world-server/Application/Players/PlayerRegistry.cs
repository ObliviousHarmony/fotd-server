using System.Collections.Concurrent;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    internal class PlayerRegistry : IPlayerRegistry
    {
        private readonly IPersistenceService _persistenceService;
        private readonly TimeProvider _timeProvider;
        private readonly ConcurrentDictionary<uint, Player> _players = new();
        private readonly ConcurrentDictionary<NetworkAddress, Player> _playersByAddress = new();
        private readonly ConcurrentDictionary<uint, PendingPlayer> _pendingPlayers = new();

        public PlayerRegistry(IPersistenceService persistenceService, TimeProvider timeProvider)
        {
            _persistenceService = persistenceService;
            _timeProvider = timeProvider;
        }

        public Player? Get(uint playerId)
        {
            return _players.GetValueOrDefault(playerId);
        }

        public Player? Get(NetworkAddress address)
        {
            return _playersByAddress.GetValueOrDefault(address);
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
                _ = _pendingPlayers.TryRemove(new KeyValuePair<uint, PendingPlayer>(playerId, pending));
                return null;
            }

            if (sender.BinaryAddress != pending.ExpectedBinaryAddress)
            {
                return null;
            }

            if (!_pendingPlayers.TryRemove(new KeyValuePair<uint, PendingPlayer>(playerId, pending)))
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
            return player;
        }

        public void Logout(Player player)
        {
            _persistenceService.WaitForPersistence(
                player,
                () =>
                {
                    _ = _playersByAddress.TryRemove(new KeyValuePair<NetworkAddress, Player>(player.Address, player));
                    _ = _players.TryRemove(new KeyValuePair<uint, Player>(player.Id, player));
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
