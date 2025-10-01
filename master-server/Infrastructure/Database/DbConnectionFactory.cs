using System.Data;
using MySqlConnector;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.Master.Core;

namespace FOMServer.Master.Infrastructure.Factories
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly DatabaseSettings dbSettings;

        public DbConnectionFactory(DatabaseSettings dbSettings)
        {
            this.dbSettings = dbSettings;
        }

        public IDbConnection Create()
        {
            return new MySqlConnection(dbSettings.ConnectionString);
        }
    }
}
