using System.Collections.Concurrent;
using FOMServer.Shared.Core.Packets;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    public class WorldLoginService : IWorldLoginService
    {
        private readonly ConcurrentDictionary<uint, PendingRequest> _pendingRequests = new();
        private readonly IPlayerRegistry _playerRegistry;

        public WorldLoginService(IPlayerRegistry playerRegistry)
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

        public WorldLoginResult? Login(uint playerID, NetworkAddress clientAddress)
        {
            if (!_pendingRequests.TryRemove(playerID, out var request))
                return null;

            var player = _playerRegistry.Register(playerID, clientAddress);
            if (player == null)
                return null;

            return new WorldLoginResult
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
