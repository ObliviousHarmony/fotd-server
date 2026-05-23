using System.Data;
using FOMServer.Shared.Infrastructure;
using FOMServer.World.Core;
using MySqlConnector;

namespace FOMServer.World.Infrastructure
{
    internal class DbConnectionFactory : IDbConnectionFactory
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
