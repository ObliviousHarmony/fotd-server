using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.Master.Application.Networking
{
    public interface IWorldPacketSender
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
