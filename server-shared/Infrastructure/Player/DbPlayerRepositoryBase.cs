using Dapper;
using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Player;
using FOMServer.Shared.Infrastructure.Database;

namespace FOMServer.Shared.Infrastructure.Player
{
    public abstract class DbPlayerRepositoryBase : IPlayerRepositoryBase
    {
        protected readonly IDbConnectionFactory _dbConnectionFactory;

        public DbPlayerRepositoryBase(IDbConnectionFactory dbConnectionFactory)
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
    }
}
