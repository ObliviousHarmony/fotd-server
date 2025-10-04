using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Extensions;

namespace FOMServer.Shared.Core.Handlers
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
        public void Handle(in Packet packet)
        {
            Handle(
                packet.Sender,
                PacketHelpers.Unwrap<TPacketData>(packet)
            );
        }

        /// <summary>
        /// Handles the data from an incoming packet.
        /// </summary>
        public abstract void Handle(NetworkAddress sender, in TPacketData data);
    }
}
