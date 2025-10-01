using System.Data;
using MySqlConnector;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.World.Core;

namespace FOMServer.World.Infrastructure.Database
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
