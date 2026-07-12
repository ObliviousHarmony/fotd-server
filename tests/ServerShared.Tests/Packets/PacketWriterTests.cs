using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets.RakNet;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Tests.Packets
{
    public class PacketWriterTests
    {
        [Fact]
        public void AddDestination_SupportsSingleAddress()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>(address);

            var packet = writer.Build();
            var addresses = packet.NetworkAddresses;

            Assert.Equal(1, addresses.Length);
            Assert.Equal(address, addresses[0]);

            packet.Release();
        }

        [Fact]
        public void AddDestination_SupportsMultipleAddresses()
        {
            var address1 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };
            var address2 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7778 };
            var address3 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7779 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>(address1);
            writer.AddDestination(address2);
            writer.AddDestination(address3);

            var packet = writer.Build();
            var addresses = packet.NetworkAddresses;

            Assert.Equal(3, addresses.Length);
            Assert.Equal(address1, addresses[0]);
            Assert.Equal(address2, addresses[1]);
            Assert.Equal(address3, addresses[2]);

            packet.Release();
        }

        [Fact]
        public void AddDestination_ThrowsInvalidOperation_TooManyPackets()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>(address);
            for (var i = 1; i < QueuePacket.MaxNetworkAddressesPerPacket; ++i)
            {
                writer.AddDestination(address);
            }

            var packet = writer.Build();

            try
            {
                writer.AddDestination(address);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException) { }

            packet.Release();
        }

        [Fact]
        public void Build_ThrowsInvalidOperation_WhenNotInitialized()
        {
            using var writer = new PacketWriter<ConnectionRequestAccepted>();

            try
            {
                writer.Build();
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException) { }
        }

        [Fact]
        public void Build_ThrowsObjectDisposed_WhenCalledTwice()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>(address);
            var packet = writer.Build();

            try
            {
                writer.Build();
                Assert.Fail("Expected ObjectDisposedException");
            }
            catch (ObjectDisposedException) { }

            packet.Release();
        }

        [Fact]
        public void Data_ThrowsObjectDisposed_AfterBuild()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>(address);
            var packet = writer.Build();

            try
            {
                _ = writer.Data;
                Assert.Fail("Expected ObjectDisposedException");
            }
            catch (ObjectDisposedException) { }

            packet.Release();
        }

        [Fact]
        public void AddDestination_ThrowsObjectDisposed_AfterBuild()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>(address);
            var packet = writer.Build();

            try
            {
                writer.AddDestination(address);
                Assert.Fail("Expected ObjectDisposedException");
            }
            catch (ObjectDisposedException) { }

            packet.Release();
        }

        [Fact]
        public void QueuePacket_ThrowsInvalidOperation_WhenNotInitialized()
        {
            var packet = new QueuePacket();

            try
            {
                packet.Release();
                Assert.Fail("Expected ObjectDisposedException");
            }
            catch (InvalidOperationException) { }
        }
    }
}
