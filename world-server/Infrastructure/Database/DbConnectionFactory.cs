using System.Data;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.World.Core;
using MySqlConnector;

namespace FOMServer.World.Infrastructure.Database
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly DatabaseSettings _dbSettings;

        public DbConnectionFactory(DatabaseSettings dbSettings)
        {
            _dbSettings = dbSettings;
        }

        public IDbConnection Create()
        {
            return new MySqlConnection(_dbSettings.ConnectionString);
        }
    }
}
