using System.Data;
using MySqlConnector;
using FOMServer.Master.Core.Models;
using FOMServer.Shared.Infrastructure.Factories;

namespace FOMServer.Master.Infrastructure.Factories
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
