using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.World.Application.Persistence
{
    internal class ItemPersistenceHandler : PersistenceHandler<Item>
    {
        private readonly ILogger<ItemPersistenceHandler> _logger;

        public ItemPersistenceHandler(ILogger<ItemPersistenceHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task PersistAsync(Item entity)
        {
            _logger.LogInformation($"Item {entity} persisted");
        }
    }
}
