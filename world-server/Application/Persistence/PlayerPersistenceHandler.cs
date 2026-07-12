using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Persistence
{
    internal class PlayerPersistenceHandler : PersistenceHandler<Player>
    {
        private readonly ILogger<PlayerPersistenceHandler> _logger;

        public PlayerPersistenceHandler(ILogger<PlayerPersistenceHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task PersistAsync(Player entity)
        {
            _logger.LogInformation("Player {PlayerId} persisted", entity.Id);
        }
    }
}
