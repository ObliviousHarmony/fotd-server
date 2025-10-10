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

        public void Send(in QueuePacket packet)
        {
            if (_packetSender == null)
                throw new InvalidOperationException("Packet sender not initialized");

            if (packet.NetworkAddresses.Length != 1 || packet.NetworkAddresses[0] != NetworkAddress.Unassigned)
                throw new InvalidOperationException("MasterPacketSender does not support sending to specific addresses");

            // Since the server is the only "connected" peer, broadcasting sends to it
            // without us needing to keep track of its address.
            _packetSender.EnqueueSend(packet.ForBroadcast());
        }
    }
}
