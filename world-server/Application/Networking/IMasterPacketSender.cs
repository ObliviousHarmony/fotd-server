using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.World.Application.Networking
{
    public interface IMasterPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send(PacketIdentifier id, FOMDataUnion data, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0);
    }
}
