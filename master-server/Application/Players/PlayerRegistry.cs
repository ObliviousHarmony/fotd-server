using System.Collections.Concurrent;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Master.Application.Players
{
    internal class PlayerRegistry : IPlayerRegistry
    {
        private readonly IPersistenceService _persistenceService;
        private readonly ConcurrentDictionary<uint, Player> _players = new();

        public PlayerRegistry(IPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
        }

        public Player? Get(uint playerId)
        {
            return _players.GetValueOrDefault(playerId);
        }

        public Player Login(ClientSession session)
        {
            if (!session.PlayerId.HasValue)
            {
                throw new InvalidOperationException("Session login must be started before it can be completed");
            }

            var playerId = session.PlayerId.Value;

            var player = new Player(playerId, session);

            if (!_players.TryAdd(playerId, player))
            {
                throw new InvalidOperationException($"Player {playerId} is already logged in");
            }

            session.CompleteLogin(player);
            _persistenceService.Register(player);
            return player;
        }

        public void Logout(Player player)
        {
            _persistenceService.WaitForPersistence(
                player,
                () => _players.TryRemove(new KeyValuePair<uint, Player>(player.Id, player)));
        }
    }
}
