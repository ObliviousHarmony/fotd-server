using System.Threading.Channels;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Networking;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
	/// Responsible for sending and receiving packets.
	/// </summary>
	public class NetworkManager : IPacketSender
    {
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

        /// <summary>
        /// A buffer for holding packets to send via the API.
        /// </summary>
        /// <remarks>
        /// Using this buffer allows us to avoid allocating a new array.
        /// We can pin the memory of the pre-allocated buffer and then
        /// pass that without having to do anything special.
        /// </remarks>
        private readonly SendPacket[] _sendBuffer = new SendPacket[IPacketService.MaxBufferedPackets];

        private IntPtr _peer;
        private Action<IntPtr>? _peerShutdown;
        private readonly IShutdownManager _shutdownManager;
        private readonly ILogService _logService;
        private readonly IPacketService _packetService;
        private readonly PacketProcessor _packetProcessor;
        private readonly Channel<SendPacket> _sendQueue;
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

            // Single writer, single reader channel is fine here
            _sendQueue = Channel.CreateUnbounded<SendPacket>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        /// <summary>
        /// Claims a packet ID for this exclusive handling by this network manager.
        /// </summary>
        public void ClaimPacketID(PacketIdentifier id)
        {
            if (!s_canClaimPacketIDs)
                throw new InvalidOperationException("Cannot claim packet IDs after a network manager has started.");

            if (s_globalClaimedPacketIDs.Contains(id))
                throw new InvalidOperationException($"Packet ID {id} is already claimed by another network manager.");

            s_globalClaimedPacketIDs.Add(id);
            _claimedPacketIDs.Add(id);
        }

        /// <summary>
        /// Configures the network manager with the necessary parameters.
        /// </summary>
        public void Configure(IntPtr peer, Action<IntPtr> peerShutdown)
        {
            if (_peer != IntPtr.Zero)
                throw new InvalidOperationException("Peer is already configured.");

            _peer = peer;
            _peerShutdown = peerShutdown;
        }

        /// <summary>
        /// Starts the network manager loop.
        /// </summary>
        public void Start()
        {
            if (_peer == IntPtr.Zero)
                throw new InvalidOperationException("Peer must be configured.");

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
                int pollingDelayMs = 1;
                while (!ct.IsCancellationRequested)
                {
                    // Avoid starving packet receiving with sending by
                    // limiting the number of packets sent per batch.
                    int numToSend = 0;
                    while (numToSend < IPacketService.MaxBufferedPackets && _sendQueue.Reader.TryRead(out var packetToSend))
                        _sendBuffer[numToSend++] = packetToSend;

                    if (numToSend > 0)
                        _packetService.Send(_peer, _sendBuffer.AsSpan(0, numToSend));

                    var received = _packetService.Receive(_peer);
                    foreach (ref readonly var packet in received)
                    {
                        // Packet IDs that have been claimed by another network manager should be ignored.
                        if (s_globalClaimedPacketIDs.Contains(packet.ID) && !_claimedPacketIDs.Contains(packet.ID))
                        {
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
            finally
            {
                _peerShutdown!(_peer);
                _peer = IntPtr.Zero;
            }
        }

        public void Send(
            PacketIdentifier id,
            FOMDataUnion data,
            NetworkAddress destination,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        )
        {
            if (_peer == IntPtr.Zero)
                throw new InvalidOperationException("Peer is not configured.");

            var packet = new SendPacket
            {
                ID = id,
                Data = data,
                NetworkAddress = destination,
                Priority = priority,
                Reliability = reliability,
                OrderingChannel = orderingChannel,
                Broadcast = 0
            };

            _sendQueue.Writer.TryWrite(packet);
        }

        public void Broadcast(
            PacketIdentifier id,
            FOMDataUnion data,
            NetworkAddress excludedAddress,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        )
        {
            if (_peer == IntPtr.Zero)
                throw new InvalidOperationException("Peer is not configured.");

            var packet = new SendPacket
            {
                ID = id,
                Data = data,
                NetworkAddress = excludedAddress,
                Priority = priority,
                Reliability = reliability,
                OrderingChannel = orderingChannel,
                Broadcast = 1
            };

            _sendQueue.Writer.TryWrite(packet);
        }
    }
}
