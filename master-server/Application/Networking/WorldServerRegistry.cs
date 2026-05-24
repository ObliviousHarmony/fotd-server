using System.Collections.Concurrent;
using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Application.Networking
{
    internal class WorldServerRegistry : IWorldServerRegistry
    {
        private readonly ConcurrentDictionary<WorldID, WorldServer> _worldServers = new();

        public WorldServer[] GetAll()
        {
            return _worldServers.Values.ToArray();
        }

        public WorldServer? Get(WorldID id)
        {
            return _worldServers.GetValueOrDefault(id);
        }

        public WorldID[] Register(WorldID[] ids, NetworkAddress serverAddress, NetworkAddress publicAddress)
        {
            var registered = new List<WorldID>();

            foreach (var id in ids)
            {
                var worldServer = new WorldServer
                {
                    ID = id,
                    ServerAddress = serverAddress,
                    PublicAddress = publicAddress
                };

                if (!_worldServers.TryAdd(id, worldServer))
                    throw new InvalidOperationException($"World {id} has already been registered");

                registered.Add(id);

            }

            return registered.ToArray();
        }

        public WorldID[] Unregister(NetworkAddress serverAddress)
        {
            var unregistered = new List<WorldID>();

            foreach (var kvp in _worldServers)
            {
                if (kvp.Value.ServerAddress.Equals(serverAddress))
                {
                    if (_worldServers.TryRemove(kvp))
                        unregistered.Add(kvp.Key);
                }
            }

            return unregistered.ToArray();
        }
    }
}
