using FOMServer.Shared.Core.Networking;

namespace FOMServer.Shared.Core.Handlers
{
    /// <summary>
    /// An interface describing a handler for incoming packets.
    /// </summary>
    public interface IPacketHandler
    {
        void Handle(in PacketRef packet);
    }
}
