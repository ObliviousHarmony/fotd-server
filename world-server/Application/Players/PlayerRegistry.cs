using FOMServer.Shared.Application.Players;
using FOMServer.Shared.Core.Packets;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
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
            var playerDTO = _playerRepository.GetByID(id);
            if (playerDTO == null)
                throw new InvalidOperationException($"Player {id} not found in database");

            var attributeDTOs = _playerRepository.GetAttributes(id);
            var attributeValues = new int[PlayerAttributes.AttributeCount];
            foreach (var attr in attributeDTOs)
                attributeValues[attr.attribute_id] = attr.value;

            var player = new Player
            {
                ID = id,
                ClientAddress = clientAddress
            };
            player.Init(attributeValues);

            return player;
        }
    }
}
