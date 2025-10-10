using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Handlers
{
    public abstract class BasePacketHandler<TPacket> : IPacketHandler where TPacket : unmanaged
    {
        public void Handle(in PacketRef packet)
        {
            Console.WriteLine($"Before Handle: {packet}");

            Handle(
                packet.Sender,
                packet.Data<TPacket>()
            );

            Console.WriteLine($"After Handle: {packet}");
        }

        public abstract void Handle(NetworkAddress sender, in TPacket packet);
    }
}
