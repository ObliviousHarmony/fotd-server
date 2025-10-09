using System.Collections.Concurrent;
using FOMServer.Shared.Core.Packets;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    public class PlayerService : IPlayerService
    {
        private readonly ConcurrentDictionary<uint, Player> _registeredPlayers;
        private readonly ConcurrentDictionary<NetworkAddress, Player> _addressMap;

        public PlayerService()
        {
            _registeredPlayers = new ConcurrentDictionary<uint, Player>();
            _addressMap = new ConcurrentDictionary<NetworkAddress, Player>();
        }

        public Player? Get(uint id)
        {
            if (!_registeredPlayers.TryGetValue(id, out var player))
                return null;
            return player;
        }

        public Player? Get(NetworkAddress clientAddress)
        {
            if (!_addressMap.TryGetValue(clientAddress, out var player))
                return null;
            return player;
        }

        public Player? OnPlayerEnteringWorld(uint id, byte selectedNodeID)
        {
            if (_registeredPlayers.ContainsKey(id))
                return null;

            var player = new Player
            {
                ID = id,
                SelectedNodeID = selectedNodeID
            };

            if (!_registeredPlayers.TryAdd(id, player))
                return null;

            return player;
        }

        public Player? OnPlayerEnteredWorld(uint id, NetworkAddress clientAddress)
        {
            if (!_registeredPlayers.TryGetValue(id, out var player))
                return null;

            player.ClientAddress = clientAddress;
            if (!_addressMap.TryAdd(clientAddress, player))
                return null;

            return player;
        }

        public void OnPlayerLeftWorld(uint id)
        {
            if (!_registeredPlayers.TryRemove(id, out var player))
                return;
            _addressMap.TryRemove(player.ClientAddress, out _);
        }
    }
}
