using System.Collections.Concurrent;
using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Master.Application.Networking
{
    public class WorldServerRegistry : IWorldServerRegistry
    {
        private readonly ConcurrentDictionary<WorldID, WorldServer> _worldServers;
        private readonly ConcurrentDictionary<NetworkAddress, WorldID> _addressMap;

        public WorldServerRegistry()
        {
            _worldServers = new ConcurrentDictionary<WorldID, WorldServer>();
            _addressMap = new ConcurrentDictionary<NetworkAddress, WorldID>();
        }

        public WorldServer[] GetAll()
        {
            return _worldServers.Values.ToArray();
        }

        public WorldServer? Get(WorldID id)
        {
            if (!_worldServers.TryGetValue(id, out var worldServer))
                return null;
            return worldServer;
        }

        public WorldServer? Get(NetworkAddress networkAddress)
        {
            if (!_addressMap.TryGetValue(networkAddress, out var id))
                return null;
            if (!_worldServers.TryGetValue(id, out var worldServer))
                return null;
            return worldServer;
        }

        public WorldServer? Register(WorldID id, NetworkAddress serverAddress, NetworkAddress clientAddress)
        {
            if (_worldServers.ContainsKey(id))
                return null;

            var worldServer = new WorldServer
            {
                ID = id,
                ServerAddress = serverAddress,
                ClientAddress = clientAddress
            };

            if (!_worldServers.TryAdd(id, worldServer))
                return null;

            if (!_addressMap.TryAdd(serverAddress, id))
            {
                _worldServers.TryRemove(id, out _);
                return null;
            }

            return worldServer;
        }

        public bool Unregister(WorldID id)
        {
            return _worldServers.TryRemove(id, out _);
        }
    }
}
