using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.World.Core.Networking;

namespace FOMServer.World.Application.Networking
{
    public class MasterPacketSender : IMasterPacketSender
    {
        private IPacketSender? _packetSender;

        public void Initialize(IPacketSender packetSender)
        {
            _packetSender = packetSender;
        }

        public void Send<TData>(
            QueuePacket.PacketData<TData> data,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        ) where TData : unmanaged
        {
            if (_packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized");

            // Since the server is the only "connected" peer, broadcasting sends to it
            // without us needing to keep track of its address.
            _packetSender.Broadcast(data, NetworkAddress.Unassigned, priority, reliability, orderingChannel);
        }
    }
}
