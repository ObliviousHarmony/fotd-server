using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Extensions;

namespace FOMServer.Shared.Application.PacketHandlers
{
    /// <summary>
    /// An abstract class for implementing packet handlers for specific packet IDs.
    /// </summary>
    /// <typeparam name="TPacketData">The data type of the packet.</typeparam>
    public abstract class PacketHandler<TPacketData> : IPacketHandler where TPacketData : unmanaged
    {
        public abstract PacketIdentifier PacketID { get; }

        /// <summary>
        /// Handles an incoming packet by extracting its data and passing it to the type-specific handler.
        /// </summary>
        /// <param name="packet">The packet to handle.</param>
        public void Handle(in FOMPacket packet)
        {
            Handle(
                packet.Sender,
                packet.GetData<TPacketData>()
            );
        }

        /// <summary>
        /// Handles the data from an incoming packet.
        /// </summary>
        /// <param name="sender">The sender of the packet.</param>
        /// <param name="data">The data property from the packet.</param>
        public abstract void Handle(NetworkAddress sender, in TPacketData data);
    }
}
