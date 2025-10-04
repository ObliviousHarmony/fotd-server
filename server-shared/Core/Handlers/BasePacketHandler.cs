using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.Shared.Core.Handlers
{
    /// <summary>
    /// An abstract class for implementing packet handlers for specific packet IDs.
    /// </summary>
    /// <typeparam name="TPacketData">The data type of the packet.</typeparam>
    public abstract class BasePacketHandler<TPacketData> : IPacketHandler where TPacketData : unmanaged
    {
        /// <summary>
        /// Handles an incoming packet by extracting its data and passing it to the type-specific handler.
        /// </summary>
        public void Handle(in Packet packet)
        {
            var unwrappedData = PacketHelpers.Unwrap<TPacketData>(packet, out var expectedID);
            if (packet.ID != expectedID)
                throw new ArgumentException($"Packet ID {packet.ID} does not match handler ID {expectedID}");

            Handle(
                packet.Sender,
                unwrappedData
            );
        }

        /// <summary>
        /// Handles the data from an incoming packet.
        /// </summary>
        public abstract void Handle(NetworkAddress sender, in TPacketData data);
    }
}
