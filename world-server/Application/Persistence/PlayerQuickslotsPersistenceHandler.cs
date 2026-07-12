using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Persistence
{
    internal class PlayerQuickslotsPersistenceHandler : PersistenceHandler<PlayerQuickslots>
    {
        private readonly ILogger<PlayerQuickslotsPersistenceHandler> _logger;

        public PlayerQuickslotsPersistenceHandler(ILogger<PlayerQuickslotsPersistenceHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task PersistAsync(PlayerQuickslots entity)
        {
            _logger.LogInformation("Player {PlayerId} quickslots persisted", entity.PlayerId);
        }
    }
}
