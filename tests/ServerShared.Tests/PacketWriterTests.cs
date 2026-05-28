using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Shared.Tests
{
    public class PacketWriterTests
    {
        [Fact]
        public void AddDestination_SupportsSingleAddress()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            writer.AddDestination(address);

            var packet = writer.Build();
            var addresses = packet.NetworkAddresses;

            Assert.Equal(1, addresses.Length);
            Assert.Equal(address, addresses[0]);
            Assert.False(packet.Broadcast);

            packet.Release();
        }

        [Fact]
        public void AddDestination_SupportsMultipleAddresses()
        {
            var address1 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };
            var address2 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7778 };
            var address3 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7779 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            writer.AddDestination(address1);
            writer.AddDestination(address2);
            writer.AddDestination(address3);

            var packet = writer.Build();
            var addresses = packet.NetworkAddresses;

            Assert.Equal(3, addresses.Length);
            Assert.Equal(address1, addresses[0]);
            Assert.Equal(address2, addresses[1]);
            Assert.Equal(address3, addresses[2]);
            Assert.False(packet.Broadcast);

            packet.Release();
        }

        [Fact]
        public void Build_DefaultsToBroadcastMode()
        {
            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            var packet = writer.Build();

            Assert.True(packet.Broadcast);

            packet.Release();
        }

        [Fact]
        public void ExcludeFromBroadcast_SetsExclusionAddress()
        {
            var excludeAddress = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            writer.ExcludeFromBroadcast(excludeAddress);

            var packet = writer.Build();
            var addresses = packet.NetworkAddresses;

            Assert.True(packet.Broadcast);
            Assert.Equal(1, addresses.Length);
            Assert.Equal(excludeAddress, addresses[0]);

            packet.Release();
        }

        [Fact]
        public void ExcludeFromBroadcast_ThrowsAfterAddDestination()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            writer.AddDestination(address);

            try
            {
                writer.ExcludeFromBroadcast(address);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Fact]
        public void AddDestination_ThrowsAfterExcludeFromBroadcast()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            writer.ExcludeFromBroadcast(address);

            try
            {
                writer.AddDestination(address);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Fact]
        public void ExcludeFromBroadcast_ThrowsWhenCalledTwice()
        {
            var address1 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };
            var address2 = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7778 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            writer.ExcludeFromBroadcast(address1);

            try
            {
                writer.ExcludeFromBroadcast(address2);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Fact]
        public void Build_ThrowsObjectDisposed_WhenCalledTwice()
        {
            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            var packet = writer.Build();

            try
            {
                _ = writer.Build();
                Assert.Fail("Expected ObjectDisposedException");
            }
            catch (ObjectDisposedException)
            {
            }

            packet.Release();
        }

        [Fact]
        public void Data_ThrowsInvalidOperation_AfterBuild()
        {
            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            var packet = writer.Build();

            try
            {
                _ = writer.Data;
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
            }

            packet.Release();
        }

        [Fact]
        public void AddDestination_ThrowsInvalidOperation_AfterBuild()
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            using var writer = new PacketWriter<ConnectionRequestAccepted>();
            var packet = writer.Build();

            try
            {
                writer.AddDestination(address);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
            }

            packet.Release();
        }
    }
}
