namespace MasterServer.Models
{
	public class ServerSettings
	{
		public ushort Port { get; init; }
	}

	public class DatabaseSettings
	{
		public string Name { get; init; } = null!;
		public string ConnectionString { get; init; } = null!;
	}
}
