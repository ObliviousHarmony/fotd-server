using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Models;
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

        public WorldServer[] GetAll()
        {
            return worldServers.Values.ToArray();
        }

        public WorldServer? Register(WorldID id, NetworkAddress serverAddress, string clientAddress, ushort clientPort)
        {
            if (worldServers.ContainsKey(id))
                return null;

            var worldServer = new WorldServer
            {
                ID = id,
                ServerAddress = serverAddress,
                ClientAddress = new NetworkAddress
                {
                    Address = clientAddress,
                    Port = clientPort,
                }
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
