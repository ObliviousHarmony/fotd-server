using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    /// <summary>
    /// Describes a service that can send packets over the network.
    /// </summary>
    public interface IPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send<TData>(
            QueuePacket.PacketData<TData> data,
            NetworkAddress destination,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged;

        /// <summary>
        /// Broadcast a packet to all connected clients.
        /// </summary>
        void Broadcast<TData>(
            QueuePacket.PacketData<TData> data,
            NetworkAddress excludedAddress,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged;
    }
}
