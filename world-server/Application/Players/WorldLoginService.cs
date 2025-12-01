using System.Collections.Concurrent;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    public class WorldLoginService : IWorldLoginService
    {
        private readonly ConcurrentDictionary<uint, WorldLoginRequest> _pendingRequests = new();

        public void AddRequest(uint playerID, byte selectedNodeID)
        {
            _pendingRequests.TryAdd(playerID, new WorldLoginRequest
            {
                PlayerID = playerID,
                SelectedNodeID = selectedNodeID
            });
        }

        public WorldLoginRequest? GetAndRemove(uint playerID)
        {
            if (_pendingRequests.TryRemove(playerID, out var request))
                return request;
            return null;
        }
    }
}
