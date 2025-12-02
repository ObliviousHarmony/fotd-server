using Dapper;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.Shared.Infrastructure.Players;
using FOMServer.World.Core.DTOs;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Infrastructure.Players
{
    public class DbPlayerRepository : DbPlayerRepositoryBase, IPlayerRepository
    {
        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        public IEnumerable<PlayerAttributeDTO> GetAttributes(uint playerID)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.Query<PlayerAttributeDTO>(
                "SELECT `attribute_id`, `value` FROM `player_attribute` WHERE `player_id` = @playerID",
                new { playerID }
            );
        }
    }
}
