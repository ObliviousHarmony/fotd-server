using FOMServer.Shared.Core.Enums;

namespace FOMServer.World.Core
{
    public class ServerSettings
    {
        public WorldID[] WorldIDs { get; init; } = [];
        public string MasterServerHost { get; init; } = null!;
        public string PublicHost { get; init; } = null!;
    }

    public class DatabaseSettings
    {
        public string Name { get; init; } = null!;
        public string ConnectionString { get; init; } = null!;
    }
}
