using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;

namespace FOMServer.Master.Core.Networking
{
    internal class WorldServer
    {
        public WorldId Id { get; init; }

        public NetworkAddress ServerAddress { get; init; }

        public NetworkAddress PublicAddress { get; init; }
    }
}
