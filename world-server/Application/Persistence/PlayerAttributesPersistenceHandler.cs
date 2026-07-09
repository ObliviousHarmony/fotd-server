using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Persistence
{
    internal class PlayerAttributesPersistenceHandler : PersistenceHandler<PlayerAttributes>
    {
        private readonly ILogger<PlayerAttributesPersistenceHandler> _logger;

        public PlayerAttributesPersistenceHandler(ILogger<PlayerAttributesPersistenceHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task PersistAsync(PlayerAttributes entity)
        {
            _logger.LogInformation($"Player {entity.PlayerId} attributes persisted");
        }
    }
}
