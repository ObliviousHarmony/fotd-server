using FOMServer.Shared.Core.Networking;

namespace FOMServer.World.Core.Networking
{
    internal interface IMasterPacketSender
    {
        void Send(in QueuePacket packet);
    }
}
