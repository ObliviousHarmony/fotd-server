using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using System.Collections.Concurrent;

namespace FOMServer.Master.Application.Networking
{
    public class WorldServerService : IWorldServerService
    {
        private readonly ConcurrentDictionary<WorldID, WorldServer> worldServers;

        public WorldServerService()
        {
            worldServers = new ConcurrentDictionary<WorldID, WorldServer>();
        }

        public WorldServer? Register(WorldID id, string clientAddress, ushort clientPort)
        {
            if (worldServers.ContainsKey(id))
                return null;

            var worldServer = new WorldServer
            {
                ID = id,
                ClientAddress = clientAddress,
                ClientPort = clientPort
            };

            if (!worldServers.TryAdd(id, worldServer))
                return null;

            return worldServer;
        }

        public bool Unregister(WorldID id)
        {
            return worldServers.TryRemove(id, out _);
        }
    }
}
