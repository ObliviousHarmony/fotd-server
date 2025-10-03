using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Networking;

namespace FOMServer.Master.Application.Networking
{
    public class ClientPacketSender : IClientPacketSender
    {
        private IPacketSender? _packetSender;

        public void Initialize(IPacketSender packetSender)
        {
            _packetSender = packetSender;
        }

        public void Send(
            PacketIdentifier id,
            FOMDataUnion data,
            NetworkAddress destination,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        )
        {
            if (_packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized");

            _packetSender.Send(id, data, destination, priority, reliability, orderingChannel);
        }

        public void Broadcast(
            PacketIdentifier id,
            FOMDataUnion data,
            NetworkAddress excludedAddress,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        )
        {
            if (_packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized");

            _packetSender.Broadcast(id, data, excludedAddress, priority, reliability, orderingChannel);
        }
    }
}
