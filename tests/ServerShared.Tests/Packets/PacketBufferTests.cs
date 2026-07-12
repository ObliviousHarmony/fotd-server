using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Utilities;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Tests.Packets
{
    public class PacketBufferTests
    {
        private static readonly PacketIdentifier s_testPacketId = PacketIdentifier.ID_CONNECTION_REQUEST_ACCEPTED;
        private static readonly int s_testPacketSize = PacketHelpers.GetPacketSize(s_testPacketId);

        [Fact]
        public unsafe void Rent_ReturnsBuffer_WhenNotAlreadyAllocated()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);

            var rentedBuffer = buffer.Rent(received);

            Assert.NotNull(rentedBuffer);
            Assert.True(rentedBuffer.Length >= s_testPacketSize);

            // Clean up by disposing the packet
            var packets = buffer.GetPackets();
            packets[0].Dispose();
        }

        [Fact]
        public unsafe void Rent_ReturnsNull_WhenAlreadyAllocated()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);

            // First rent should succeed
            var firstRent = buffer.Rent(received);
            Assert.NotNull(firstRent);

            // Second rent should fail
            var secondRent = buffer.Rent(received);
            Assert.Null(secondRent);

            // Clean up
            var packets = buffer.GetPackets();
            packets[0].Dispose();
        }

        [Fact]
        public void GetPackets_ThrowsInvalidOperation_WhenNotAllocated()
        {
            var buffer = new PacketBuffer();

            Assert.Throws<InvalidOperationException>(() => buffer.GetPackets());
        }

        [Fact]
        public unsafe void DisposePacket_FreesBuffer_WhenLastPacketDisposed()
        {
            var buffer = new PacketBuffer();
            var identifiers = stackalloc PacketIdentifier[2] { s_testPacketId, s_testPacketId };
            var senders = stackalloc NetworkAddress[2]
            {
                new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 },
                new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7778 }
            };

            var received = CreateReceivedPackets(2, identifiers, senders);
            buffer.Rent(received);

            var packets = buffer.GetPackets();
            Assert.Equal(2, packets.Length);

            // Dispose first packet - buffer should still be allocated
            packets[0].Dispose();

            // Dispose second packet - buffer should now be freed
            packets[1].Dispose();

            // After all packets disposed, we should be able to rent again
            var secondRent = buffer.Rent(received);
            Assert.NotNull(secondRent);

            // Clean up
            var newPackets = buffer.GetPackets();
            foreach (var packet in newPackets)
            {
                packet.Dispose();
            }
        }

        [Fact]
        public unsafe void DisposePacket_ThrowsObjectDisposed_WhenCalledTwice()
        {
            var buffer = new PacketBuffer();

            // Use 2 packets so the buffer stays allocated after disposing the first one.
            // This ensures we hit the disposal flag check rather than the refCount check.
            var identifiers = stackalloc PacketIdentifier[2] { s_testPacketId, s_testPacketId };
            var senders = stackalloc NetworkAddress[2]
            {
                new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 },
                new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7778 }
            };

            var received = CreateReceivedPackets(2, identifiers, senders);
            buffer.Rent(received);

            var packets = buffer.GetPackets();
            var packet = packets[0];

            // First dispose should succeed
            packet.Dispose();

            // Second dispose should throw ObjectDisposedException
            Assert.Throws<ObjectDisposedException>(packet.Dispose);

            // Clean up the second packet
            packets[1].Dispose();
        }

        [Fact]
        public unsafe void DisposePacket_ThrowsObjectDisposed_WhenBufferVersionMismatch()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);
            buffer.Rent(received);

            var packets = buffer.GetPackets();
            var stalePacket = packets[0];

            // Dispose to free the buffer and increment version
            stalePacket.Dispose();

            // Rent again - this increments the buffer version
            buffer.Rent(received);
            var newPackets = buffer.GetPackets();

            // The stale packet reference should throw because version mismatches
            Assert.Throws<ObjectDisposedException>(stalePacket.Dispose);

            // Clean up new allocation
            newPackets[0].Dispose();
        }

        [Fact]
        public unsafe void PacketRef_ID_ThrowsObjectDisposed_WhenAccessingDisposedPacket()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);
            buffer.Rent(received);

            var packets = buffer.GetPackets();
            var packet = packets[0];

            packet.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = packet.Id);
        }

        [Fact]
        public unsafe void PacketRef_Sender_ThrowsObjectDisposed_WhenAccessingDisposedPacket()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);
            buffer.Rent(received);

            var packets = buffer.GetPackets();
            var packet = packets[0];

            packet.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = packet.Sender);
        }

        [Fact]
        public unsafe void PacketRef_ReturnsCorrectValues_BeforeDispose()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);
            buffer.Rent(received);

            var packets = buffer.GetPackets();
            var packet = packets[0];

            Assert.Equal(s_testPacketId, packet.Id);
            Assert.Equal(sender, packet.Sender);

            // Clean up
            packet.Dispose();
        }

        [Fact]
        public unsafe void PacketRef_ToString_ReturnsDisposedMessage_AfterDispose()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);
            buffer.Rent(received);

            var packets = buffer.GetPackets();
            var packet = packets[0];

            packet.Dispose();

            var str = packet.ToString();
            Assert.Contains("<disposed>", str);
        }

        [Fact]
        public unsafe void PacketRef_Status_ReturnsReadError_WhenStatusByteIsNonZero()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);
            var rentedBuffer = buffer.Rent(received);

            // Simulate a read error by setting the status byte
            rentedBuffer![0] = (byte)SerializationStatus.ReadError;

            var packets = buffer.GetPackets();
            Assert.Equal(SerializationStatus.ReadError, packets[0].Status);

            packets[0].Dispose();
        }

        [Fact]
        public unsafe void PacketRef_Data_ThrowsInvalidOperation_WhenStatusIsNotSuccess()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);
            var rentedBuffer = buffer.Rent(received);

            // Simulate a read error
            rentedBuffer![0] = (byte)SerializationStatus.ReadError;

            var packets = buffer.GetPackets();
            var packet = packets[0];

            Assert.Throws<InvalidOperationException>(() => packet.Data<ConnectionRequestAccepted>());

            packet.Dispose();
        }

        [Fact]
        public unsafe void PacketRef_ToString_ReturnsErrorMessage_WhenStatusIsNotSuccess()
        {
            var buffer = new PacketBuffer();
            var identifier = s_testPacketId;
            var sender = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            var received = CreateReceivedPackets(1, &identifier, &sender);
            var rentedBuffer = buffer.Rent(received);

            // Simulate a read error
            rentedBuffer![0] = (byte)SerializationStatus.ReadError;

            var packets = buffer.GetPackets();
            var str = packets[0].ToString();

            Assert.Contains("<error: ReadError>", str);

            packets[0].Dispose();
        }

        /// <summary>
        /// Creates a ReceivedPackets struct for testing with the specified number of packets.
        /// </summary>
        private unsafe ReceivedPackets CreateReceivedPackets(
            byte count,
            PacketIdentifier* identifiers,
            NetworkAddress* senders)
        {
            return new ReceivedPackets
            {
                Count = count,
                Packets = IntPtr.Zero, // Not used by PacketBuffer.Rent
                Identifiers = identifiers,
                Senders = senders
            };
        }
    }
}
