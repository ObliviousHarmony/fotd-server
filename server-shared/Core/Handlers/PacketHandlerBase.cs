using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Shared.Core.Handlers
{
    public abstract class PacketHandlerBase<TPacket> : IPacketHandler where TPacket : unmanaged
    {
        public void Handle(in PacketRef packet)
        {
            Handle(
                packet.Sender,
                packet.Data<TPacket>()
            );
        }

        public abstract void Handle(NetworkAddress sender, in TPacket packet);
    }
}
