using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.World.Core.Networking
{
    public interface IMasterPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send(
            PacketIdentifier id,
            FOMDataUnion data,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        );
    }
}
