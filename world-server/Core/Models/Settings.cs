namespace FOMServer.World.Core.Models
{
    public class ServerSettings
    {
        public byte WorldID { get; init; }
        public ushort ClientPort { get; init; }
        public string MasterServer { get; init; } = null!;
        public ushort MasterServerPort { get; init; }
    }

    public class DatabaseSettings
    {
        public string Name { get; init; } = null!;
        public string ConnectionString { get; init; } = null!;
    }
}
