using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Shared.Core.Networking
{
    public interface IPacketSender
    {
        void EnqueueSend(in QueuePacket packet);

        void CloseConnection(in NetworkAddress address);
    }
}
