using System.Data;
using FOMServer.Master.Core;
using FOMServer.Shared.Infrastructure.Database;
using MySqlConnector;

namespace FOMServer.Master.Infrastructure.Factories
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly DatabaseSettings _dbSettings;

        public DbConnectionFactory(DatabaseSettings dbSettings)
        {
            this._dbSettings = dbSettings;
        }

        public IDbConnection Create()
        {
            return new MySqlConnection(_dbSettings.ConnectionString);
        }
    }
}
