using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Interop.FOMNetwork;

namespace FOMServer.Master.Core.Networking
{
    internal interface IClientPacketSender
    {
        void Send(in QueuePacket packet);

        void CloseConnection(in NetworkAddress address);
    }
}
