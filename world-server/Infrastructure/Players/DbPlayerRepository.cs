using Dapper;
using FOMServer.Shared.Core.DTOs;
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

        public PlayerDTO? GetByID(uint id)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.QuerySingleOrDefault<PlayerDTO>(
                "SELECT `id`, `username` FROM `player` WHERE `id` = @id",
                new { id }
            );
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
