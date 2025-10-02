using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Master.Core.Networking
{
    public class WorldServer
    {
        public WorldID ID { get; init; }
        public NetworkAddress ClientAddress { get; init; }
    }
}
