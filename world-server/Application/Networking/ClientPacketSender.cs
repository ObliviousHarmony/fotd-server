using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.World.Core.Networking;

namespace FOMServer.World.Application.Networking
{
    internal class ClientPacketSender : IClientPacketSender
    {
        private IPacketSender? _packetSender;

        public void Initialize(IPacketSender packetSender)
        {
            _packetSender = packetSender;
        }

        public void Send(in QueuePacket packet)
        {
            if (_packetSender is null)
            {
                throw new InvalidOperationException("Packet sender not initialized");
            }

            _packetSender.EnqueueSend(packet);
        }

        public void CloseConnection(in NetworkAddress address)
        {
            if (_packetSender is null)
            {
                throw new InvalidOperationException("Packet sender not initialized");
            }

            _packetSender.CloseConnection(in address);
        }
    }
}
