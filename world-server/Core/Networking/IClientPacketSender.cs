using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Interop.FOMNetwork;

namespace FOMServer.World.Core.Networking
{
    internal interface IClientPacketSender
    {
        void Send(in QueuePacket packet);

        void CloseConnection(in NetworkAddress address);
    }
}
