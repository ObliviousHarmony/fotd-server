using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Interop.FOMNetwork;

namespace FOMServer.Shared.Core.PacketHandlers
{
    public abstract class PacketHandlerBase<TPacket> : IPacketHandler
        where TPacket : unmanaged
    {
        public void Handle(in PacketRef packet)
        {
            Handle(packet.Sender, packet.Data<TPacket>());
        }

        public abstract void Handle(NetworkAddress sender, in TPacket packet);
    }
}
