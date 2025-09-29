namespace FOMServer.Master.Core.Models
{
    public class ServerSettings
    {
        public ushort WorldPort { get; init; }
        public ushort ClientPort { get; init; }
    }

    public class DatabaseSettings
    {
        public string Name { get; init; } = null!;
        public string ConnectionString { get; init; } = null!;
    }
}
