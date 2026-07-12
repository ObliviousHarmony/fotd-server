using FOMServer.Shared.Core.Networking;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Master.Core.Networking
{
    internal interface IClientPacketSender
    {
        void Send(in QueuePacket packet);

        void CloseConnection(in NetworkAddress address);
    }
}
