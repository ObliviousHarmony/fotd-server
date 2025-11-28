using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data.RakNetPackets;

namespace FOMServer.Tests
{
    public class QueuePacketTests
    {
        [Fact]
        public void Data_ThrowsObjectDisposed_AfterDispose()
        {
            using var builder = new PacketBuilder<ConnectionRequestAccepted>();
            var packet = builder.Build();

            packet.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = packet.Data);
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes_Safely()
        {
            using var builder = new PacketBuilder<ConnectionRequestAccepted>();
            var packet = builder.Build();

            packet.Dispose();

            // Second dispose should not throw
            var exception = Record.Exception(() => packet.Dispose());

            Assert.Null(exception);
        }

        [Fact]
        public void NetworkAddresses_ReturnsSingleAddress_WhenOneProvided()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var builder = new PacketBuilder<ConnectionRequestAccepted>();
            builder.WithAddress(address);
            var packet = builder.Build();

            var addresses = packet.NetworkAddresses;

            Assert.Equal(1, addresses.Length);
            Assert.Equal(address, addresses[0]);

            packet.Dispose();
        }

        [Fact]
        public void NetworkAddresses_ReturnsAllAddresses_WhenMultipleProvided()
        {
            var address1 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };
            var address2 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7778 };

            using var builder = new PacketBuilder<ConnectionRequestAccepted>();
            builder
                .WithAddress(address1)
                .WithAddress(address2);

            var packet = builder.Build();
            var addresses = packet.NetworkAddresses;

            Assert.Equal(2, addresses.Length);
            Assert.Equal(address1, addresses[0]);
            Assert.Equal(address2, addresses[1]);

            packet.Dispose();
        }

        [Fact]
        public void ForBroadcast_CreatesCopyWithBroadcastFlag()
        {
            using var builder = new PacketBuilder<ConnectionRequestAccepted>();
            var packet = builder.Build();

            Assert.False(packet.IsBroadcast);

            var broadcastPacket = packet.ForBroadcast();

            Assert.True(broadcastPacket.IsBroadcast);

            // Original packet's broadcast flag is unchanged (it's a copy)
            Assert.False(packet.IsBroadcast);

            packet.Dispose();
        }
    }
}
