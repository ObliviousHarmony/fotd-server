using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.Shared.Core.Handlers
{
    /// <summary>
    /// An interface describing a handler for incoming packets.
    /// </summary>
    public interface IPacketHandler
    {
        void Handle(in Packet packet);
    }
}
