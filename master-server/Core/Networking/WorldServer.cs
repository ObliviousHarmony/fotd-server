using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Master.Core.Networking
{
    public class WorldServer
    {
        public WorldID ID { get; init; }
        public NetworkAddress ServerAddress { get; init; }
        public NetworkAddress ClientAddress { get; init; }
    }
}
