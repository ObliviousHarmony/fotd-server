using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.World.Application.Networking
{
    /// <summary>
    /// Responsible for handling the connection to the master server.
    /// </summary>
    public class MasterConnectionManager : NetworkManager
    {
        public MasterConnectionManager(
            ILogService logService,
            IPacketService packetService,
            PacketProcessor packetProcessor
        ) : base(logService, packetService, packetProcessor)
        {
        }
    }
}
