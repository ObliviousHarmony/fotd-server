using System.Data;
using FOMServer.Master.Core;
using FOMServer.Shared.Infrastructure;
using MySqlConnector;

namespace FOMServer.Master.Infrastructure
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
