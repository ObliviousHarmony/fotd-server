using FOMServer.Shared.Core.Enums;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Master.Core.Networking
{
    internal class WorldServer
    {
        public WorldId Id { get; init; }

        public NetworkAddress ServerAddress { get; init; }

        public NetworkAddress PublicAddress { get; init; }
    }
}
