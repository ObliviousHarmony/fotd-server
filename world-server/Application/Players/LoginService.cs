using System.Collections.Concurrent;
using FOMServer.Shared.Core.Packets;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    public class LoginService : ILoginService
    {
        private readonly ConcurrentDictionary<uint, PendingRequest> _pendingRequests = new();
        private readonly IPlayerRegistry _playerRegistry;

        public LoginService(IPlayerRegistry playerRegistry)
        {
            _playerRegistry = playerRegistry;
        }

        public void Prepare(uint playerID, byte selectedNodeID)
        {
            _pendingRequests.TryAdd(playerID, new PendingRequest
            {
                PlayerID = playerID,
                SelectedNodeID = selectedNodeID
            });
        }

        public LoginContext? Login(uint playerID, NetworkAddress clientAddress)
        {
            if (!_pendingRequests.TryGetValue(playerID, out var request))
                return null;

            var player = _playerRegistry.Register(playerID, clientAddress);

            // Don't remove the request until after the player registration attempt was made.
            _pendingRequests.Remove(playerID, out _);

            if (player == null)
                return null;

            return new LoginContext
            {
                Player = player,
                SelectedNodeID = request.SelectedNodeID
            };
        }

        private class PendingRequest
        {
            public uint PlayerID { get; init; }
            public byte SelectedNodeID { get; init; }
        }
    }
}
