using Dapper;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Infrastructure.Factories;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbPlayerRepository : IPlayerRepository
    {
        private IConnectionFactory connectionFactory;

        public DbPlayerRepository(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public uint? FindIDByName(string name)
        {
            using var connection = connectionFactory.Create();
            return connection.QueryFirstOrDefault<uint?>("SELECT `id` FROM `player` WHERE `name` = @name", new { name });
        }
    }
}
