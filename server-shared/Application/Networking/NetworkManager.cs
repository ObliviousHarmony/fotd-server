using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
	/// Responsible for sending and receiving packets.
	/// </summary>
	public class NetworkManager : IPacketSender
    {
        /// <summary>
        /// Rather than pinning each packet's memory individually, we copy all of
        /// the packet data to a contiguous buffer and pin that instead. This
        /// is the maximum size of each individual instance of that buffer
        /// so that we don't end up using too much memory.
        /// </summary>
        private const int MaxSendBufferSize = 1024 * 1024;

        /// <summary>
        /// Individual network managers can "claim" packet IDs so that they
        /// exclusively handle them. When another network manager receives
        /// a packet with a claimed ID, it will ignore it.
        /// </summary>
        private static readonly HashSet<PacketIdentifier> s_globalClaimedPacketIDs = new HashSet<PacketIdentifier>();

        /// <summary>
        /// Packet ID claims are not using a thread-safe collection for
        /// performance reasons. Claims should be done during
        /// initialization and be prevented once the first
        /// network manager has started.
        /// </summary>
        private static bool s_canClaimPacketIDs = true;

        private IntPtr _peer;
        private Action<IntPtr>? _peerShutdown;
        private readonly IShutdownManager _shutdownManager;
        private readonly ILogService _logService;
        private readonly IPacketService _packetService;
        private readonly PacketProcessor _packetProcessor;
        private readonly Channel<QueuePacket> _sendQueue;
        private readonly HashSet<PacketIdentifier> _claimedPacketIDs;
        private Task? _networkTask;
        private CancellationTokenSource? _cts;

        public NetworkManager(
            IShutdownManager shutdownManager,
            ILogService logService,
            IPacketService packetService,
            PacketProcessor packetProcessor
        )
        {
            _peer = IntPtr.Zero;
            _shutdownManager = shutdownManager;
            _logService = logService;
            _packetService = packetService;
            _packetProcessor = packetProcessor;
            _claimedPacketIDs = new HashSet<PacketIdentifier>();
            _sendQueue = Channel.CreateUnbounded<QueuePacket>(
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            );
        }

        /// <summary>
        /// Claims a packet ID for this exclusive handling by this network manager.
        /// </summary>
        public void ClaimPacketID(PacketIdentifier id)
        {
            if (!s_canClaimPacketIDs)
                throw new InvalidOperationException("Cannot claim packet IDs after a network manager has started");

            if (s_globalClaimedPacketIDs.Contains(id))
                throw new InvalidOperationException($"Packet ID {id} is already claimed by another network manager");

            s_globalClaimedPacketIDs.Add(id);
            _claimedPacketIDs.Add(id);
        }

        /// <summary>
        /// Configures the network manager with the necessary parameters.
        /// </summary>
        public void Configure(IntPtr peer, Action<IntPtr> peerShutdown)
        {
            if (_peer != IntPtr.Zero)
                throw new InvalidOperationException("Peer is already configured");

            _peer = peer;
            _peerShutdown = peerShutdown;
        }

        /// <summary>
        /// Starts the network manager loop.
        /// </summary>
        public void Start()
        {
            if (_peer == IntPtr.Zero)
                throw new InvalidOperationException("Peer must be configured");

            if (_networkTask != null)
                return;

            // Once a network manager has started, no more claims can be made.
            s_canClaimPacketIDs = false;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownManager.Token);

            // Use a dedicated thread for this task because we need to
            // keep polling the network library to maximize throughput.
            _networkTask = Task.Factory.StartNew(
                async () => await NetworkLoopAsync(_cts.Token),
                _cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            ).Unwrap();

            // Make sure that the shutdown manager waits for this task to complete.
            _shutdownManager.TrackTask(_networkTask);
        }

        private async Task NetworkLoopAsync(CancellationToken ct)
        {
            try
            {
                // Rather than sending each packet individually, we batch them together to reduce
                // the number of calls into the native networking library. In order to do this,
                // we need to hold onto the packets we pull off the queue until after we've
                // sent them.
                QueuePacket[] queueBuffer = new QueuePacket[IPacketService.MaxBufferedPackets];

                // Sending packets to the networking library requires pinning the memory they
                // occupy so that the garbage collector doesn't move them around while the
                // native code is accessing them. Since pinning memory is expensive, we
                // copy the memory for all of the packets into a contiguous buffer and
                // pin that buffer instead.
                ArrayPool<byte> sendPool = ArrayPool<byte>.Create(MaxSendBufferSize, IPacketService.MaxBufferedPackets);
                byte[][] rentedBuffers = new byte[IPacketService.MaxBufferedPackets][];
                SendPacket[] sendBuffer = new SendPacket[IPacketService.MaxBufferedPackets];
                GCHandle[] rentedBufferHandles = new GCHandle[IPacketService.MaxBufferedPackets];
                ArrayPool<NetworkAddress> networkAddressPool = ArrayPool<NetworkAddress>.Create();

                int pollingDelayMs = 1;
                while (!ct.IsCancellationRequested)
                {
                    // Avoid starving packet receiving with sending by
                    // limiting the number of packets sent per batch.
                    int numToSend = 0;
                    int bufferSize = 0;
                    int numRentedBuffers = 0;
                    int numNetworkAddresses = 0;
                    while (numToSend < IPacketService.MaxBufferedPackets && _sendQueue.Reader.TryRead(out queueBuffer[numToSend]))
                    {
                        ref readonly var packetToSend = ref queueBuffer[numToSend++];

                        var packetSize = PacketHelpers.GetPacketSize(packetToSend.ID);
                        if (packetSize > MaxSendBufferSize)
                            throw new InvalidOperationException($"Packet ID {packetToSend.ID} is too large to send ({packetSize} bytes)");

                        numNetworkAddresses += packetToSend.NetworkAddresses.Length;

                        // Support overflow into multiple buffers if needed.
                        if (bufferSize + packetSize > MaxSendBufferSize)
                        {
                            rentedBuffers[numRentedBuffers++] = sendPool.Rent(bufferSize);
                            bufferSize = 0;
                        }

                        bufferSize += packetSize;
                    }

                    if (numToSend > 0)
                    {
                        // Rent a buffer for any remaining packets.
                        if (bufferSize > 0)
                            rentedBuffers[numRentedBuffers++] = sendPool.Rent(bufferSize);

                        // We support a variable number of network addresses per packet. As a result,
                        // they need to be copied into a contiguous block so that we can pin them
                        // all at once rather than pinning each individual packet's addresses.
                        var networkAddresses = networkAddressPool.Rent(numNetworkAddresses);
                        var networkAddressesHandle = GCHandle.Alloc(networkAddresses, GCHandleType.Pinned);

                        unsafe
                        {
                            // Begin by pinning all of the buffers so that the garbage collector
                            // doesn't move them around while the native code is accessing them.
                            for (int i = 0; i < numRentedBuffers; ++i)
                                rentedBufferHandles[i] = GCHandle.Alloc(rentedBuffers[i], GCHandleType.Pinned);

                            int bufferIndex = 0;
                            int bufferOffset = 0;
                            int networkAddressOffset = 0;
                            NetworkAddress* networkAddressPtr = (NetworkAddress*)networkAddressesHandle.AddrOfPinnedObject();
                            for (int i = 0; i < numToSend; ++i)
                            {
                                ref readonly var packetToSend = ref queueBuffer[i];

                                // We already allocated extra buffers for overflow above, so
                                // if we overflow here, move on to the next buffer.
                                var packetSize = PacketHelpers.GetPacketSize(packetToSend.ID);
                                if (bufferOffset + packetSize > MaxSendBufferSize)
                                {
                                    bufferOffset = 0;
                                    bufferIndex++;
                                }

                                // Copy all of the packet's network addresses into the block.
                                int numAddresses = 0;
                                foreach (var address in packetToSend.NetworkAddresses)
                                    networkAddresses[networkAddressOffset + (numAddresses++)] = address;

                                // Copy the packet data into the current buffer.
                                byte* bufferPtr = (byte*)rentedBufferHandles[bufferIndex].AddrOfPinnedObject();
                                fixed (byte* srcPtr = packetToSend.Data)
                                {
                                    Unsafe.CopyBlockUnaligned(
                                        bufferPtr + bufferOffset,
                                        srcPtr,
                                        (uint)packetSize
                                    );

                                    sendBuffer[i] = new SendPacket
                                    {
                                        ID = packetToSend.ID,
                                        Data = (IntPtr)(bufferPtr + bufferOffset),
                                        NumNetworkAddresses = numAddresses,
                                        NetworkAddresses = (IntPtr)(networkAddressPtr + networkAddressOffset),
                                        Priority = packetToSend.Priority,
                                        Reliability = packetToSend.Reliability,
                                        OrderingChannel = packetToSend.OrderingChannel,
                                        Broadcast = (byte)(packetToSend.Broadcast ? 1 : 0)
                                    };
                                }
                                bufferOffset += packetSize;
                                networkAddressOffset += numAddresses;

                                // We don't need the packet's data anymore since we copied it.
                                packetToSend.Release();
                            }

                            _packetService.Send(_peer, sendBuffer.AsSpan(0, numToSend));
                        }

                        // Free all of the handles since we're done with the packets.
                        networkAddressesHandle.Free();
                        for (int i = 0; i < numRentedBuffers; ++i)
                            rentedBufferHandles[i].Free();

                        // We're done with the network addresses now that we've sent the packets.
                        networkAddressPool.Return(networkAddresses);

                        // Return any rented buffers back to the pool.
                        for (int i = 0; i < numRentedBuffers; ++i)
                            sendPool.Return(rentedBuffers[i]);
                    }

                    // Poll for incoming packets.
                    var received = _packetService.Receive(_peer);
                    foreach (ref readonly var packet in received)
                    {
                        // Packet IDs that have been claimed by another network manager should be ignored.
                        if (s_globalClaimedPacketIDs.Contains(packet.ID) && !_claimedPacketIDs.Contains(packet.ID))
                        {
                            packet.Dispose();
                            _logService.WriteMessage(LogLevel.Error, $"Client {packet.Sender} sent packet with claimed ID {packet.ID}, ignoring.");
                            continue;
                        }

                        _packetProcessor.Enqueue(packet);
                    }

                    // Use an exponential back-off strategy when polling to avoid
                    // wasting CPU when idle while still being responsive to
                    // periodic bursts of activity.
                    if (numToSend > 0 || received.Length > 0)
                        pollingDelayMs = 1;
                    else
                        pollingDelayMs = Math.Min(pollingDelayMs * 2, 100);

                    await Task.Delay(pollingDelayMs, ct);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logService.WriteMessage(LogLevel.Critical, $"Network Failure: {ex}");
                _shutdownManager.StartShutdown();
            }
            finally
            {
                _peerShutdown!(_peer);
                _peer = IntPtr.Zero;
            }
        }

        public void EnqueueSend(in QueuePacket packet)
        {
            if (_peer == IntPtr.Zero)
                throw new InvalidOperationException("Peer is not configured");

            _sendQueue.Writer.TryWrite(packet);
        }
    }
}
