using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.World.Application.Networking
{
    public class MasterPacketSender : IMasterPacketSender
    {
        private IPacketSender? packetSender;

        public void Initialize(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Send(PacketIdentifier id, FOMDataUnion data, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0)
        {
            if (packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized.");

            // Since the server is the only "connected" peer, broadcasting sends to it
            // without us needing to keep track of its address.
            packetSender.Broadcast(id, data, NetworkAddress.Unassigned, priority, reliability, orderingChannel);
        }
    }
}
