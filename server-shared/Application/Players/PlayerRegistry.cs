using System.Collections.Concurrent;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Players;

namespace FOMServer.Shared.Application.Players
{
    /// <summary>
    /// Abstract base registry for tracking players present on a server.
    /// </summary>
    public abstract class PlayerRegistryBase<TPlayer> : IPlayerRegistry<TPlayer>
        where TPlayer : PlayerBase
    {
        private readonly ConcurrentDictionary<uint, TPlayer> _players;
        private readonly ConcurrentDictionary<NetworkAddress, TPlayer> _addressMap;

        protected PlayerRegistryBase()
        {
            _players = new ConcurrentDictionary<uint, TPlayer>();
            _addressMap = new ConcurrentDictionary<NetworkAddress, TPlayer>();
        }

        public TPlayer? Get(uint id)
        {
            if (!_players.TryGetValue(id, out var player))
                return null;
            return player;
        }

        public TPlayer? Get(NetworkAddress clientAddress)
        {
            if (!_addressMap.TryGetValue(clientAddress, out var player))
                return null;
            return player;
        }

        public TPlayer? Register(uint id, NetworkAddress clientAddress)
        {
            if (_players.ContainsKey(id))
                return null;

            if (_addressMap.ContainsKey(clientAddress))
                return null;

            var player = Load(id, clientAddress);

            if (!_players.TryAdd(id, player))
                return null;

            if (!_addressMap.TryAdd(clientAddress, player))
            {
                _players.TryRemove(id, out _);
                return null;
            }

            return player;
        }

        public void Unregister(uint id)
        {
            if (!_players.TryRemove(id, out var player))
                return;
            _addressMap.TryRemove(player.ClientAddress, out _);
        }

        /// <summary>
        /// Loads a player instance. Override to load additional data from repositories.
        /// </summary>
        protected abstract TPlayer Load(uint id, NetworkAddress clientAddress);
    }
}
