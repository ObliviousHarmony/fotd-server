using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.World.Core.Networking
{
    public interface IClientPacketSender
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
