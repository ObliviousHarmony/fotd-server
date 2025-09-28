using System.Data;
using MySqlConnector;
using FOMServer.World.Core.Models;
using FOMServer.Shared.Infrastructure.Factories;

namespace FOMServer.World.Infrastructure.Factories
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly DatabaseSettings dbSettings;

        public ConnectionFactory(DatabaseSettings dbSettings)
        {
            this.dbSettings = dbSettings;
        }

        public IDbConnection Create()
        {
            return new MySqlConnection(dbSettings.ConnectionString);
        }
    }
}
