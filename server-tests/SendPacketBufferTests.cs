using System.Buffers;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Infrastructure.FOMNetwork;

namespace FOMServer.Tests
{
    public class SendPacketBufferTests
    {
        private static readonly PacketIdentifier s_testPacketID = PacketIdentifier.ID_CONNECTION_REQUEST_ACCEPTED;
        private static readonly int s_testPacketSize = PacketHelpers.GetPacketSize(s_testPacketID);

        [Fact]
        public void Add_GetBatch_ReturnsPackets()
        {
            var buffer = new SendPacketBuffer();
            var packet1 = CreateTestPacket();
            var packet2 = CreateTestPacket();

            buffer.Add(in packet1);
            buffer.Add(in packet2);
            var batch = buffer.GetBatch();

            Assert.Equal(2, batch.Length);
        }

        [Fact]
        public void Add_CopiesPacketFields()
        {
            var buffer = new SendPacketBuffer();
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };
            var packet = CreateTestPacket(
                address: address,
                priority: PacketPriority.High,
                reliability: PacketReliability.Reliable,
                orderingChannel: 5,
                broadcast: false
            );

            buffer.Add(in packet);
            var batch = buffer.GetBatch();

            Assert.Equal(s_testPacketID, batch[0].ID);
            Assert.Equal(PacketPriority.High, batch[0].Priority);
            Assert.Equal(PacketReliability.Reliable, batch[0].Reliability);
            Assert.Equal(5, batch[0].OrderingChannel);
            Assert.Equal(0, batch[0].Broadcast);
            Assert.Equal(1, batch[0].NumNetworkAddresses);
        }

        [Fact]
        public void Add_WithMultipleAddresses_CopiesAllAddresses()
        {
            var buffer = new SendPacketBuffer();
            var addresses = new NetworkAddress[]
            {
                new() { BinaryAddress = 0x0100007F, Port = 7777 },
                new() { BinaryAddress = 0x0200007F, Port = 7778 },
                new() { BinaryAddress = 0x0300007F, Port = 7779 }
            };
            var packet = CreateTestPacketWithMultipleAddresses(addresses);

            buffer.Add(in packet);
            var batch = buffer.GetBatch();

            Assert.Equal(3, batch[0].NumNetworkAddresses);
        }

        [Fact]
        public void Add_ThrowsInvalidOperation_WhenBufferFull()
        {
            var buffer = new SendPacketBuffer();

            // Fill the buffer to capacity
            for (int i = 0; i < IPacketService.MaxBufferedPackets; i++)
            {
                var packet = CreateTestPacket();
                buffer.Add(in packet);
            }

            Assert.False(buffer.CanAdd);

            // Next add should throw
            var overflowPacket = CreateTestPacket();
            Assert.Throws<InvalidOperationException>(() => buffer.Add(in overflowPacket));

            // Clean up the overflow packet since it wasn't added
            overflowPacket.Release();
        }

        private static QueuePacket CreateTestPacket(
            NetworkAddress? address = null,
            PacketPriority priority = PacketPriority.Medium,
            PacketReliability reliability = PacketReliability.ReliableOrdered,
            byte orderingChannel = 0,
            bool broadcast = false
        )
        {
            var packetData = ArrayPool<byte>.Shared.Rent(s_testPacketSize);
            var networkAddress = address ?? new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            return new QueuePacket(
                s_testPacketID,
                packetData,
                networkAddress,
                networkAddresses: null,
                addressCount: 1,
                priority,
                reliability,
                orderingChannel,
                broadcast
            );
        }

        private static QueuePacket CreateTestPacketWithMultipleAddresses(NetworkAddress[] addresses)
        {
            var packetData = ArrayPool<byte>.Shared.Rent(s_testPacketSize);
            var rentedAddresses = ArrayPool<NetworkAddress>.Shared.Rent(addresses.Length);
            addresses.CopyTo(rentedAddresses, 0);

            return new QueuePacket(
                s_testPacketID,
                packetData,
                NetworkAddress.Unassigned,
                rentedAddresses,
                addresses.Length,
                PacketPriority.Medium,
                PacketReliability.ReliableOrdered,
                orderingChannel: 0,
                broadcast: false
            );
        }
    }
}
