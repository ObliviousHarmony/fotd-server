using FOMServer.Shared.Core.Enums;

namespace FOMServer.World.Core.Networking
{
    public interface IMasterPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send<TData>(
            TData data,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged;
    }
}
