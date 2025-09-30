using FOMServer.Master.Core.Interfaces;
using FOMServer.Master.Core.Models;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using System.Collections.Concurrent;

namespace FOMServer.Master.Application.Services
{
    public class WorldServerService : IWorldServerService
    {
        private readonly ConcurrentDictionary<WorldID, WorldServer> worldServers;

        public WorldServerService()
        {
            this.worldServers = new ConcurrentDictionary<WorldID, WorldServer>();
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
