using Dapper;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.Shared.Infrastructure.Players;

namespace FOMServer.Master.Infrastructure.Players
{
    public class DbPlayerRepository : DbPlayerRepositoryBase, IPlayerRepository
    {
        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        public uint? GetIDByUsername(string username)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.QuerySingleOrDefault<uint?>(
                "SELECT `id` FROM `player` WHERE `username` = @username",
                new { username }
            );
        }
    }
}
