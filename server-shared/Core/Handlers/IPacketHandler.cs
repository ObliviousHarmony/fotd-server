using FOMServer.Shared.Core.Networking;

namespace FOMServer.Shared.Core.Handlers
{
    public interface IPacketHandler
    {
        void Handle(in PacketRef packet);
    }
}
