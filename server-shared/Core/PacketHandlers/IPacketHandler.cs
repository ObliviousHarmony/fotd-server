using FOMServer.Shared.Application.Networking;

namespace FOMServer.Shared.Core.PacketHandlers
{
    public interface IPacketHandler
    {
        void Handle(in PacketRef packet);
    }
}
