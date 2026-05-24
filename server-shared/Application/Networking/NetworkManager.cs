using System.Threading.Channels;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Infrastructure.FOMNetwork;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
	/// Responsible for sending and receiving packets.
	/// </summary>
	public partial class NetworkManager : IPacketSender, IAsyncDisposable
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

        private IntPtr _peer;
        private Action<IntPtr>? _peerShutdown;
        private readonly IShutdownManager _shutdownManager;
        private readonly ILogger<NetworkManager> _logger;
        private readonly IPacketService _packetService;
        private readonly PacketProcessor _packetProcessor;
        private readonly Channel<QueuePacket> _sendQueue;
        private readonly HashSet<PacketIdentifier> _claimedPacketIDs = [];
        private Task? _networkTask;
        private CancellationTokenSource? _cts;

        public NetworkManager(
            IShutdownManager shutdownManager,
            ILogger<NetworkManager> logger,
            IPacketService packetService,
            PacketProcessor packetProcessor
        )
        {
            _shutdownManager = shutdownManager;
            _logger = logger;
            _packetService = packetService;
            _packetProcessor = packetProcessor;
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

        public void EnqueueSend(in QueuePacket packet)
        {
            if (_peer == IntPtr.Zero)
                throw new InvalidOperationException("Peer is not configured");

            _sendQueue.Writer.TryWrite(packet);
        }

        public async ValueTask DisposeAsync()
        {
            _sendQueue.Writer.Complete();

            if (_networkTask != null)
                await _networkTask;

            if (_peer != IntPtr.Zero)
            {
                _peerShutdown!(_peer);
                _peer = IntPtr.Zero;
            }
        }

        private async Task NetworkLoopAsync(CancellationToken ct)
        {
            try
            {
                var sendBuffer = new SendPacketBuffer();

                // We use an exponential back-off strategy to maintain throughput
                // while avoiding wasted CPU cycles doing nothing.
                var pollingDelayMs = 1;
                while (!ct.IsCancellationRequested)
                {
                    var shouldBackoff = true;

                    // Avoid starving packet receiving with sending by
                    // limiting the number of packets sent per batch.
                    while (sendBuffer.CanAdd && _sendQueue.Reader.TryRead(out var packetToSend))
                        sendBuffer.Add(in packetToSend);

                    if (sendBuffer.HasBatch)
                    {
                        _packetService.Send(_peer, sendBuffer.GetBatch());
                        sendBuffer.Reset();
                        shouldBackoff = false;
                    }

                    // Poll for incoming packets.
                    var received = _packetService.Receive(_peer);
                    foreach (ref readonly var packet in received)
                    {
                        // Packets that failed to deserialize should not be processed.
                        if (packet.Status != SerializationStatus.Success)
                        {
                            LogMalformedPacket(packet.Sender, packet.ID, packet.Status);
                            packet.Dispose();
                            continue;
                        }

                        // Packet IDs that have been claimed by another network manager should be ignored.
                        if (s_globalClaimedPacketIDs.Contains(packet.ID) && !_claimedPacketIDs.Contains(packet.ID))
                        {
                            LogClaimedPacketID(packet.Sender, packet.ID);
                            packet.Dispose();
                            continue;
                        }

                        _packetProcessor.Enqueue(packet);
                        shouldBackoff = false;
                    }

                    if (shouldBackoff)
                        pollingDelayMs = Math.Min(pollingDelayMs * 2, 100);
                    else
                        pollingDelayMs = 1;

                    await Task.Delay(pollingDelayMs, ct);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                LogNetworkFailure(ex);
                _shutdownManager.StartShutdown();
            }
            finally
            {
                _peerShutdown!(_peer);
                _peer = IntPtr.Zero;
            }
        }

        [LoggerMessage(Level = LogLevel.Warning, Message = "Client {Sender} sent malformed packet with ID {PacketID}: {Status}")]
        private partial void LogMalformedPacket(NetworkAddress sender, PacketIdentifier packetID, SerializationStatus status);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Client {Sender} sent packet with claimed ID {PacketID}, ignoring")]
        private partial void LogClaimedPacketID(NetworkAddress sender, PacketIdentifier packetID);

        [LoggerMessage(Level = LogLevel.Critical, Message = "Network Failure")]
        private partial void LogNetworkFailure(Exception ex);
    }
}
