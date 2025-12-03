using FOMServer.Shared.Core.Networking;

namespace FOMServer.World.Core.Networking
{
    public interface IClientPacketSender
    {
        void Send(in QueuePacket packet);
        void Broadcast(in QueuePacket packet);
    }
}
