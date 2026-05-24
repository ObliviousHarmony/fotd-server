using FOMServer.Shared.Core.Networking;
using FOMServer.World.Core.Networking;

namespace FOMServer.World.Application.Networking
{
    internal class MasterPacketSender : IMasterPacketSender
    {
        private IPacketSender? _packetSender;

        public void Initialize(IPacketSender packetSender)
        {
            _packetSender = packetSender;
        }

        public void Send(in QueuePacket packet)
        {
            if (_packetSender is null)
                throw new InvalidOperationException("Packet sender not initialized");

            if (!packet.Broadcast)
                throw new InvalidOperationException("Packet must not have a destination");

            _packetSender.EnqueueSend(packet);
        }
    }
}
