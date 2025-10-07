using System.Collections.Concurrent;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Master.Application.Players
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ICharacterRepository _characterRepository;

        private readonly ConcurrentDictionary<uint, Player> _loggedIn;
        private readonly ConcurrentDictionary<NetworkAddress, Player> _addressMap;

        public PlayerService(IPlayerRepository playerRepository, ICharacterRepository characterRepository)
        {
            _playerRepository = playerRepository;
            _loggedIn = new ConcurrentDictionary<uint, Player>();
            _addressMap = new ConcurrentDictionary<NetworkAddress, Player>();
            _characterRepository = characterRepository;
        }

        public Player? Get(uint id)
        {
            if (!_loggedIn.TryGetValue(id, out var player))
                return null;
            return player;
        }

        public Player? Get(NetworkAddress clientAddress)
        {
            if (!_addressMap.TryGetValue(clientAddress, out var player))
                return null;
            return player;
        }

        public Player? Login(string username, string password, NetworkAddress clientAddress)
        {
            var dto = _playerRepository.TryLogin(username, password);
            if (dto == null)
                return null;

            if (_loggedIn.ContainsKey(dto.id))
                return null;

            if (_addressMap.ContainsKey(clientAddress))
                return null;

            var player = new Player
            {
                ClientAddress = clientAddress,
                ID = dto.id,
                Username = dto.username
            };

            if (!_loggedIn.TryAdd(dto.id, player))
                return null;

            if (!_addressMap.TryAdd(clientAddress, player))
            {
                _loggedIn.TryRemove(dto.id, out _);
                return null;
            }

            var character = _characterRepository.Get(player.ID);
            player.HasCharacter = character != null;

            return player;
        }

        public bool Logout(Player player)
        {
            if (!_loggedIn.ContainsKey(player.ID))
                return false;

            if (!_loggedIn.TryRemove(player.ID, out _))
                return false;

            _addressMap.TryRemove(player.ClientAddress, out _);

            _playerRepository.Logout(player.ID);

            return true;
        }
    }
}
