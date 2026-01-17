using Dapper;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.Shared.Infrastructure.Player;
using FOMServer.World.Core.Player;

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
