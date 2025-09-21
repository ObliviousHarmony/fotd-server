using System.Data;
using FOMServer.Shared.Factories;
using FOMServer.Master.Models;
using MySqlConnector;

namespace FOMServer.Master.Factories
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
