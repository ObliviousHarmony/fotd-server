using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
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

        public void Send<TData>(
            QueuePacket.PacketData<TData> data,
            NetworkAddress destination,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged
        {
            if (_packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized");

            _packetSender.Send(data, destination, priority, reliability, orderingChannel);
        }

        public void Broadcast<TData>(
            QueuePacket.PacketData<TData> data,
            NetworkAddress excludedAddress,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged
        {
            if (_packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized");

            _packetSender.Broadcast(data, excludedAddress, priority, reliability, orderingChannel);
        }
    }
}
