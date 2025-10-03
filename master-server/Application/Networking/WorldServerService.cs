using System.Collections.Concurrent;
using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.Master.Application.Networking
{
    public class WorldServerService : IWorldServerService
    {
        private readonly ConcurrentDictionary<WorldID, WorldServer> _worldServers;

        public WorldServerService()
        {
            _worldServers = new ConcurrentDictionary<WorldID, WorldServer>();
        }

        public WorldServer[] GetAll()
        {
            return _worldServers.Values.ToArray();
        }

        public WorldServer? Register(WorldID id, NetworkAddress serverAddress, string clientAddress, ushort clientPort)
        {
            if (_worldServers.ContainsKey(id))
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

            if (!_worldServers.TryAdd(id, worldServer))
                return null;

            return worldServer;
        }

        public bool Unregister(WorldID id)
        {
            return _worldServers.TryRemove(id, out _);
        }
    }
}
