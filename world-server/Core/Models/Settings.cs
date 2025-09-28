namespace FOMServer.World.Core.Models
{
    public class ServerSettings
    {
        public byte WorldID { get; init; }
        public ushort Port { get; init; }
        public string MasterServer { get; init; } = null!;
    }

    public class DatabaseSettings
    {
        public string Name { get; init; } = null!;
        public string ConnectionString { get; init; } = null!;
    }
}
