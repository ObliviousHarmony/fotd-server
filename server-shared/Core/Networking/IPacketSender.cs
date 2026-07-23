using FOMServer.Shared.Interop.FOMNetwork;

namespace FOMServer.Shared.Core.Networking
{
    public interface IPacketSender
    {
        void EnqueueSend(in QueuePacket packet);

        void CloseConnection(in NetworkAddress address);
    }
}
