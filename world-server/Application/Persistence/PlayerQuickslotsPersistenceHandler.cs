using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Infrastructure;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Persistence
{
    internal class PlayerQuickslotsPersistenceHandler : PersistenceHandler<PlayerQuickslots>
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public PlayerQuickslotsPersistenceHandler(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected override async Task PersistAsync(PlayerQuickslots entity)
        {
            var quickslots = new ItemType[PlayerConstants.NumQuickslots];
            entity.CopyTo(quickslots);

            using var connection = _dbConnectionFactory.Create();

            await connection.ExecuteAsync(
                """
                UPDATE `player`
                SET `quickslot_1` = @quickslot1,
                  `quickslot_2` = @quickslot2,
                  `quickslot_3` = @quickslot3,
                  `quickslot_4` = @quickslot4
                WHERE `id` = @id
                """,
                new
                {
                    id = entity.PlayerId,
                    quickslot1 = quickslots[0],
                    quickslot2 = quickslots[1],
                    quickslot3 = quickslots[2],
                    quickslot4 = quickslots[3],
                }
            );
        }
    }
}
