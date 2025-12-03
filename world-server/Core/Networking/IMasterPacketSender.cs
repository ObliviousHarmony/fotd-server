using FOMServer.Shared.Core.Networking;

namespace FOMServer.World.Core.Networking
{
    public interface IMasterPacketSender
    {
        void Send(in QueuePacket packet);
    }
}
