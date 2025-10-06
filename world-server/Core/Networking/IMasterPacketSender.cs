using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;

namespace FOMServer.World.Core.Networking
{
    public interface IMasterPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send<TData>(
            QueuePacket.PacketData<TData> data,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged;
    }
}
