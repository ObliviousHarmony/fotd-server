using Dapper;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.World.Core.DTOs;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Infrastructure.Players
{
    public class DbPlayerRepository : IPlayerRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public IEnumerable<PlayerAttributeDto> GetAttributes(uint playerID)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.Query<PlayerAttributeDto>(
                "SELECT `attribute_id`, `value` FROM `player_attribute` WHERE `player_id` = @playerID",
                new { playerID }
            );
        }
    }
}
