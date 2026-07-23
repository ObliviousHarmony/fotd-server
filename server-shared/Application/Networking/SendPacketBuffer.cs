using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
    /// Collects a batch of packets to hand to native code in a single send.
    /// </summary>
    internal class SendPacketBuffer
    {
        private readonly SendPacket[] _sendPackets;
        private readonly NetworkAddress[] _networkAddresses;
        private readonly QueuePacket[] _pendingRelease;

        private int _networkAddressOffset;
        private int _packetCount;

        public SendPacketBuffer()
        {
            _sendPackets = new SendPacket[IPacketService.MaxBufferedPackets];
            _pendingRelease = new QueuePacket[IPacketService.MaxBufferedPackets];

            // Stage addresses on the pinned object heap so native receives a stable
            // pointer without us pinning per send.
            _networkAddresses = GC.AllocateArray<NetworkAddress>(
                IPacketService.MaxBufferedPackets * QueuePacket.MaxNetworkAddressesPerPacket,
                pinned: true
            );
        }

        public bool CanAdd => _packetCount < IPacketService.MaxBufferedPackets;

        public bool HasBatch => _packetCount > 0;

        /// <summary>
        /// Adds a packet to the send buffer, referencing its pinned data in place
        /// and staging its addresses.
        /// </summary>
        /// <remarks>
        /// The packet is NOT released here. Native reads its buffer in place during
        /// the send, so packets are released via <see cref="ReleasePending"/> only
        /// after the batch has been sent.
        /// </remarks>
        public unsafe bool Add(in QueuePacket packet)
        {
            if (_packetCount >= IPacketService.MaxBufferedPackets)
            {
                throw new InvalidOperationException("Cannot add more packets to the buffer");
            }

            var addresses = packet.NetworkAddresses;

            // Packets with no specific destination should be sent to all connected clients.
            var broadcast = false;
            if (addresses.Length == 1 && addresses[0] == NetworkAddress.Unassigned)
            {
                broadcast = true;
            }

            // Stage the addresses into the pinned buffer so native has a stable pointer.
            for (var i = 0; i < addresses.Length; i++)
            {
                _networkAddresses[_networkAddressOffset + i] = addresses[i];
            }

            // Reference the packet's pinned data buffer directly (no copy), plus the
            // staged addresses.
            fixed (NetworkAddress* addrPtr = &_networkAddresses[_networkAddressOffset])
            {
                _sendPackets[_packetCount] = new SendPacket
                {
                    Id = packet.Id,
                    Data = packet.DataPointer,
                    NumNetworkAddresses = addresses.Length,
                    NetworkAddresses = (IntPtr)addrPtr,
                    Priority = packet.Priority,
                    Reliability = packet.Reliability,
                    OrderingChannel = packet.OrderingChannel,
                    Broadcast = (byte)(broadcast ? 1 : 0),
                };
            }

            // Hold the packet so its buffer stays rented until the batch is sent.
            _pendingRelease[_packetCount] = packet;

            _networkAddressOffset += addresses.Length;
            _packetCount++;

            return true;
        }

        public ReadOnlySpan<SendPacket> GetBatch()
        {
            return _sendPackets.AsSpan(0, _packetCount);
        }

        /// <summary>
        /// Releases the buffers of every packet in the current batch. Call after the
        /// batch has been sent, once native no longer references the data.
        /// </summary>
        public void ReleasePending()
        {
            for (var i = 0; i < _packetCount; i++)
            {
                _pendingRelease[i].Release();

                // Drop the reference to the returned buffer so
                // that it can be released if necessary.
                _pendingRelease[i] = default;
            }

            _networkAddressOffset = 0;
            _packetCount = 0;
        }
    }
}
