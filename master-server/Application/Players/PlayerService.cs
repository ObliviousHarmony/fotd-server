using FOMServer.Master.Core.Players;
using FOMServer.Master.Core.Repositories;
using FOMServer.Shared.Core.FOMPacket.Models;
using System.Collections.Concurrent;

namespace FOMServer.Master.Application.Players
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository playerRepository;

        private readonly ConcurrentDictionary<uint, Player> loggedIn;
        private readonly ConcurrentDictionary<NetworkAddress, Player> addressMap;

        public PlayerService(IPlayerRepository playerRepository)
        {
            this.playerRepository = playerRepository;
            loggedIn = new ConcurrentDictionary<uint, Player>();
            addressMap = new ConcurrentDictionary<NetworkAddress, Player>();
        }

        public Player? Get(uint id)
        {
            if (!loggedIn.TryGetValue(id, out var player))
                return null;
            return player;
        }

        public Player? Get(NetworkAddress clientAddress)
        {
            if (!addressMap.TryGetValue(clientAddress, out var player))
                return null;
            return player;
        }

        public Player? Login(string username, string password, NetworkAddress clientAddress)
        {
            var dto = playerRepository.TryLogin(username, password);
            if (dto == null)
                return null;

            if (loggedIn.ContainsKey(dto.id))
                return null;

            if (addressMap.ContainsKey(clientAddress))
                return null;

            var player = new Player
            {
                ClientAddress = clientAddress,
                ID = dto.id,
                Username = dto.username
            };

            if (!loggedIn.TryAdd(dto.id, player))
                return null;

            if (!addressMap.TryAdd(clientAddress, player))
            {
                loggedIn.TryRemove(dto.id, out _);
                return null;
            }

            return player;
        }

        public bool Logout(Player player)
        {
            if (!loggedIn.ContainsKey(player.ID))
                return false;

            if (!loggedIn.TryRemove(player.ID, out _))
                return false;

            addressMap.TryRemove(player.ClientAddress, out _);

            playerRepository.Logout(player.ID);

            return true;
        }
    }
}
