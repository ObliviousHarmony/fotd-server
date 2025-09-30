using System.Data;
using MySqlConnector;
using FOMServer.World.Core.Models;
using FOMServer.Shared.Infrastructure.Factories;

namespace FOMServer.World.Infrastructure.Factories
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
