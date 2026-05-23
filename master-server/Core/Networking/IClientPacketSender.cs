using FOMServer.Shared.Core.Networking;

namespace FOMServer.Master.Core.Networking
{
    internal interface IClientPacketSender
    {
        void Send(in QueuePacket packet);
        void Broadcast(in QueuePacket packet);
    }
}
