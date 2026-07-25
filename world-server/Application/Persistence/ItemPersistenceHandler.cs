using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Infrastructure;

namespace FOMServer.World.Application.Persistence
{
    internal class ItemPersistenceHandler : PersistenceHandler<Item>
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ItemPersistenceHandler(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected override async Task PersistAsync(Item entity)
        {
            using var connection = _dbConnectionFactory.Create();

            var snapshot = entity.ToSnapshot();

            await connection.ExecuteAsync(
                """
                UPDATE `item`
                SET `value` = @value,
                    `durability` = @durability,
                    `slot` = @slot,
                    `location_type` = @locationType,
                    `location_id` = @locationId,
                    `deleted_at` = IF(@deleted, CURRENT_TIMESTAMP, `deleted_at`)
                WHERE `id` = @id
                """,
                new
                {
                    id = snapshot.Id,
                    value = snapshot.Value,
                    durability = snapshot.Durability,
                    slot = snapshot.Slot,
                    locationType = snapshot.LocationType,
                    locationId = snapshot.LocationId,
                    deleted = snapshot.IsDeleted,
                }
            );
        }
    }
}
