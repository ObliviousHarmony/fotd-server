using FOMServer.Shared.Core.Enums;

namespace FOMServer.World.Core
{
    public class ServerSettings
    {
        public WorldID WorldID { get; init; }
        public string ClientAddress { get; init; } = null!;
        public ushort ClientPort { get; init; }
        public string MasterServerAddress { get; init; } = null!;
        public ushort MasterServerPort { get; init; }
    }

    public class DatabaseSettings
    {
        public string Name { get; init; } = null!;
        public string ConnectionString { get; init; } = null!;
    }
}
