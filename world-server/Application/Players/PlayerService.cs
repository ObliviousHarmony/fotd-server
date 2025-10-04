using System.Collections.Concurrent;
using FOMServer.Shared.Core.FOMPacket;
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

        public Player? OnPlayerEnteringWorld(uint playerID, byte selectedNodeID)
        {
            if (_registeredPlayers.ContainsKey(playerID))
                return null;

            var player = new Player
            {
                ID = playerID,
                SelectedNodeID = selectedNodeID
            };

            if (!_registeredPlayers.TryAdd(playerID, player))
                return null;

            return player;
        }
    }
}
