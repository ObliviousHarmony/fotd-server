using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Handlers
{
    public abstract class BasePacketHandler<TPacket> : IPacketHandler where TPacket : unmanaged
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
