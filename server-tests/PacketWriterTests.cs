using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data.RakNetPackets;

namespace FOMServer.Tests
{
    public class PacketWriterTests
    {
        [Fact]
        public void AddAddress_SupportsSingleAddress()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            writer.AddAddress(address);

            using var packet = writer.Build();
            var addresses = packet.NetworkAddresses;

            Assert.Equal(1, addresses.Length);
            Assert.Equal(address, addresses[0]);
        }

        [Fact]
        public void AddAddress_SupportsMultipleAddresses()
        {
            var address1 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };
            var address2 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7778 };
            var address3 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7779 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            writer.AddAddress(address1);
            writer.AddAddress(address2);
            writer.AddAddress(address3);

            using var packet = writer.Build();
            var addresses = packet.NetworkAddresses;

            Assert.Equal(3, addresses.Length);
            Assert.Equal(address1, addresses[0]);
            Assert.Equal(address2, addresses[1]);
            Assert.Equal(address3, addresses[2]);
        }

        [Fact]
        public void Build_ThrowsObjectDisposed_WhenCalledTwice()
        {
            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            using var packet = writer.Build();

            Assert.Throws<ObjectDisposedException>(() => writer.Build());
        }

        [Fact]
        public void Data_ThrowsInvalidOperation_AfterBuild()
        {
            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            using var packet = writer.Build();

            Assert.Throws<InvalidOperationException>(() => _ = writer.Data);
        }

        [Fact]
        public void AddAddress_ThrowsInvalidOperation_AfterBuild()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            using var packet = writer.Build();

            Assert.Throws<InvalidOperationException>(() => writer.AddAddress(address));
        }

        [Fact]
        public void Priority_ThrowsInvalidOperation_AfterBuild()
        {
            var writer = new PacketWriter<ConnectionRequestAccepted>();
            using var packet = writer.Build();

            Assert.Throws<InvalidOperationException>(() => writer.Priority = PacketPriority.High);
        }

        [Fact]
        public void Reliability_ThrowsInvalidOperation_AfterBuild()
        {
            var writer = new PacketWriter<ConnectionRequestAccepted>();
            using var packet = writer.Build();

            Assert.Throws<InvalidOperationException>(() => writer.Reliability = PacketReliability.Unreliable);
        }

        [Fact]
        public void OrderingChannel_ThrowsInvalidOperation_AfterBuild()
        {
            var writer = new PacketWriter<ConnectionRequestAccepted>();
            using var packet = writer.Build();

            Assert.Throws<InvalidOperationException>(() => writer.OrderingChannel = 5);
        }

        [Fact]
        public void ForBroadcast_CreatesCopyWithBroadcastFlag()
        {
            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            using var packet = writer.Build();

            Assert.False(packet.IsBroadcast);

            var broadcastPacket = packet.ForBroadcast();

            Assert.True(broadcastPacket.IsBroadcast);
            Assert.False(packet.IsBroadcast); // Original unchanged
        }
    }
}
