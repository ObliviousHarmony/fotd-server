using FOMServer.Master.Core.Players;
using FOMServer.Shared.Application.Players;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Master.Application.Players
{
    public class PlayerRegistry : BasePlayerRegistry<Player>, IPlayerRegistry
    {
        private readonly IPlayerRepository _playerRepository;

        public PlayerRegistry(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        protected override Player Load(uint id, NetworkAddress clientAddress)
        {
            var playerDto = _playerRepository.GetByID(id);
            if (playerDto == null)
                throw new InvalidOperationException($"Player {id} not found in database");

            var avatar = _playerRepository.GetAvatar(id);

            return new Player
            {
                ID = id,
                ClientAddress = clientAddress,
                Username = playerDto.username,
                HasAvatar = avatar != null
            };
        }
    }
}
