using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.World.Application.Networking
{
    public class ClientPacketSender : IClientPacketSender
    {
        private IPacketSender? packetSender;

        public void Initialize(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Send(PacketIdentifier id, FOMDataUnion data, NetworkAddress destination, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0)
        {
            if (packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized.");

            packetSender.Send(id, data, destination, priority, reliability, orderingChannel);
        }

        public void Broadcast(PacketIdentifier id, FOMDataUnion data, NetworkAddress excludedAddress, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0)
        {
            if (packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized.");

            packetSender.Broadcast(id, data, excludedAddress, priority, reliability, orderingChannel);
        }
    }
}
