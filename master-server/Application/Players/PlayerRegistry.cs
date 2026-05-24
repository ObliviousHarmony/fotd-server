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

        public Player? Get(uint playerID) => _players.GetValueOrDefault(playerID);

        public Player Login(ClientSession session)
        {
            if (!session.PlayerID.HasValue)
                throw new InvalidOperationException("Session login must be started before it can be completed");

            var playerID = session.PlayerID.Value;

            var player = new Player(playerID, session);

            if (!_players.TryAdd(playerID, player))
                throw new InvalidOperationException($"Player {playerID} is already logged in");

            session.CompleteLogin(player);
            _persistenceService.Register(player);
            return player;
        }

        public void Logout(Player player)
        {
            _persistenceService.WaitForPersistence(
                player,
                () => _players.TryRemove(new KeyValuePair<uint, Player>(player.ID, player)));
        }
    }
}
