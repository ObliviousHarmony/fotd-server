using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Infrastructure.FOMNetwork;

namespace FOMServer.World.Application.Networking
{
    /// <summary>
    /// Responsible for handling the connection to the master server.
    /// </summary>
    public class MasterConnectionManager : NetworkManager
    {
        public MasterConnectionManager(
            IPacketService packetService,
            PacketProcessor packetProcessor
        ) : base(packetService, packetProcessor)
        {
        }
    }
}
