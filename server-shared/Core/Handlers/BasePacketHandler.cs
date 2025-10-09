using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;

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
        public void Handle(in PacketRef packet)
        {
            Console.WriteLine("Before Handle");
            packet.PrintContent();
            if (packet.ID == Enums.PacketIdentifier.ID_LOGIN_REQUEST)
            if (typeof(TPacketData) == typeof(LoginRequest))
            {
                LoginRequest data = packet.Data<LoginRequest>();
                 Console.WriteLine($"Username {data.Username}");
            }

            Handle(
                packet.Sender,
                packet.Data<TPacketData>()
            );

            Console.WriteLine("After Handle");
            packet.PrintContent();
        }

        /// <summary>
        /// Handles the data from an incoming packet.
        /// </summary>
        public abstract void Handle(NetworkAddress sender, in TPacketData packet);
    }
}
