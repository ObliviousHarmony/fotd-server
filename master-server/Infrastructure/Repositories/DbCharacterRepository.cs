using Dapper;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Infrastructure.Factories;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbCharacterRepository : ICharacterRepository
    {
        private IConnectionFactory connectionFactory;

        public DbCharacterRepository(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public uint? IsNameTaken(string name)
        {
            using var connection = connectionFactory.Create();
            return connection.QueryFirstOrDefault<uint?>("SELECT `id` FROM `character` WHERE `name` = @name", new { name });
        }
    }
}
