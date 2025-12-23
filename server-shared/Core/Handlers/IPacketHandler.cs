using FOMServer.Shared.Application.Networking;

namespace FOMServer.Shared.Core.Handlers
{
    public interface IPacketHandler
    {
        void Handle(in PacketRef packet);
    }
}
