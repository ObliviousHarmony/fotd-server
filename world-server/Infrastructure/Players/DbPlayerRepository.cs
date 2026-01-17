using Dapper;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.Shared.Infrastructure.Players;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Infrastructure.Players
{
    public class DbPlayerRepository : DbPlayerRepositoryBase, IPlayerRepository
    {
        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }
    }
}
