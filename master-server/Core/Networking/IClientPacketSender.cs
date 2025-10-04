using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.Master.Core.Networking
{
    public interface IClientPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send<TData>(
            TData data,
            NetworkAddress destination,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged;

        /// <summary>
        /// Broadcast a packet to all connected clients.
        /// </summary>
        void Broadcast<TData>(
            TData data,
            NetworkAddress excludedAddress,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged;
    }
}
