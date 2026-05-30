using System.Collections.Concurrent;
using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Application.Networking
{
    internal class WorldServerRegistry : IWorldServerRegistry
    {
        private readonly ConcurrentDictionary<WorldId, WorldServer> _worldServers = new();

        public WorldServer[] GetAll()
        {
            return [.. _worldServers.Values];
        }

        public WorldServer? Get(WorldId id)
        {
            return _worldServers.GetValueOrDefault(id);
        }

        public WorldId[] Register(WorldId[] ids, NetworkAddress serverAddress, NetworkAddress publicAddress)
        {
            var registered = new List<WorldId>();

            foreach (var id in ids)
            {
                var worldServer = new WorldServer
                {
                    Id = id,
                    ServerAddress = serverAddress,
                    PublicAddress = publicAddress
                };

                if (!_worldServers.TryAdd(id, worldServer))
                {
                    throw new InvalidOperationException($"World '{id}' has already been registered");
                }

                registered.Add(id);
            }

            return [.. registered];
        }

        public WorldId[] Unregister(NetworkAddress serverAddress)
        {
            var unregistered = new List<WorldId>();

            foreach (var kvp in _worldServers)
            {
                if (kvp.Value.ServerAddress.Equals(serverAddress))
                {
                    if (_worldServers.TryRemove(kvp))
                    {
                        unregistered.Add(kvp.Key);
                    }
                }
            }

            return [.. unregistered];
        }
    }
}
