using FOMServer.Master.Core.Networking;
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

        public void Send(in QueuePacket packet)
        {
            if (_packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized");

            _packetSender.EnqueueSend(packet);
        }

        public void Broadcast(in QueuePacket packet)
        {
            if (_packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized");

            _packetSender.EnqueueSend(packet.ForBroadcast());
        }
    }
}
