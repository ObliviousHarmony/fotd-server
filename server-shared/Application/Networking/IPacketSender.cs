using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
    /// Describes a service that can send packets over the network.
    /// </summary>
    public interface IPacketSender
    {
        /// <summary>
        /// Sends a packet over the network.
        /// </summary>
        void Send(PacketIdentifier id, FOMDataUnion data, NetworkAddress destination, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0);

        /// <summary>
        /// Broadcast a packet to all connected clients.
        /// </summary>
        void Broadcast(PacketIdentifier id, FOMDataUnion data, NetworkAddress excludedAddress, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0);
    }
}
