using FOMServer.Shared.Application.Players;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Players;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    public class PlayerRegistry : PlayerRegistryBase<Player>, IPlayerRegistry
    {
        private readonly IPlayerRepository _playerRepository;

        public PlayerRegistry(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        protected override Player Load(uint id, NetworkAddress clientAddress)
        {
            var playerDTO = _playerRepository.GetByID(id);
            if (playerDTO == null)
                throw new InvalidOperationException($"Player {id} not found in database");

            var session = new PlayerSession(id, clientAddress);

            return new Player(session);
        }
    }
}
