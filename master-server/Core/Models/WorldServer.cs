using FOMServer.Shared.Core.Enums;

namespace FOMServer.Master.Core.Models
{
    public class WorldServer
    {
        public WorldID ID { get; init; }
        public string ClientAddress { get; init; } = null!;
        public ushort ClientPort { get; init; }
    }
}
