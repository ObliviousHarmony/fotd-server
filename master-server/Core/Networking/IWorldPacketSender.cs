using FOMServer.Shared.Core.Networking;

namespace FOMServer.Master.Core.Networking
{
    internal interface IWorldPacketSender
    {
        void Send(in QueuePacket packet);
    }
}
