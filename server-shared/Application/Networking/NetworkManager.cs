using System.Threading.Channels;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
	/// Responsible for sending and receiving packets.
	/// </summary>
	public partial class NetworkManager : IPacketSender, IAsyncDisposable
    {
        /// <summary>
        /// Individual network managers can "claim" packet Ids so that they
        /// exclusively handle them. When another network manager receives
        /// a packet with a claimed Id, it will ignore it.
        /// </summary>
        private static readonly Dictionary<PacketIdentifier, PacketClaimBehavior> s_globalClaimedPacketIds = [];

        /// <summary>
        /// Packet Id claims are not using a thread-safe collection for
        /// performance reasons. Claims should be done during
        /// initialization and be prevented once the first
        /// network manager has started.
        /// </summary>
        private static bool s_canClaimPacketIds = true;

        private IntPtr _peer;
        private Action<IntPtr>? _peerShutdown;
        private readonly IShutdownManager _shutdownManager;
        private readonly ILogger<NetworkManager> _logger;
        private readonly IPacketService _packetService;
        private readonly PacketProcessor _packetProcessor;
        private readonly Channel<QueuePacket> _sendQueue;
        private readonly Channel<NetworkAddress> _closeConnectionQueue;
        private readonly HashSet<PacketIdentifier> _claimedPacketIds = [];
        private Task? _networkTask;
        private CancellationTokenSource? _cts;

        public NetworkManager(
            IShutdownManager shutdownManager,
            ILogger<NetworkManager> logger,
            IPacketService packetService,
            PacketProcessor packetProcessor)
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
            _closeConnectionQueue = Channel.CreateUnbounded<NetworkAddress>(
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            );
        }

        /// <summary>
        /// Controls what other network managers do when they receive a packet
        /// whose Id this manager has claimed.
        /// </summary>
        public enum PacketClaimBehavior
        {
            Warn,
            IgnoreSilently,
        }

        /// <summary>
        /// Claims a packet Id for this exclusive handling by this network manager.
        /// </summary>
        public void ClaimPacketId(
            PacketIdentifier id,
            PacketClaimBehavior behavior = PacketClaimBehavior.Warn)
        {
            if (!s_canClaimPacketIds)
            {
                throw new InvalidOperationException("Cannot claim packet Ids after a network manager has started");
            }

            if (s_globalClaimedPacketIds.ContainsKey(id))
            {
                throw new InvalidOperationException($"Packet Id '{id}' is already claimed by another network manager");
            }

            s_globalClaimedPacketIds.Add(id, behavior);
            _claimedPacketIds.Add(id);
        }

        /// <summary>
        /// Configures the network manager with the necessary parameters.
        /// </summary>
        public void Configure(IntPtr peer, Action<IntPtr> peerShutdown)
        {
            if (_peer != IntPtr.Zero)
            {
                throw new InvalidOperationException("Peer is already configured");
            }

            _peer = peer;
            _peerShutdown = peerShutdown;
        }

        /// <summary>
        /// Starts the network manager loop.
        /// </summary>
        public void Start()
        {
            if (_peer == IntPtr.Zero)
            {
                throw new InvalidOperationException("Peer must be configured");
            }

            if (_networkTask is not null)
            {
                return;
            }

            // Once a network manager has started, no more claims can be made.
            s_canClaimPacketIds = false;

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
            {
                throw new InvalidOperationException("Peer is not configured");
            }

            _sendQueue.Writer.TryWrite(packet);
        }

        public void CloseConnection(in NetworkAddress address)
        {
            if (_peer == IntPtr.Zero)
            {
                throw new InvalidOperationException("Peer is not configured");
            }

            _closeConnectionQueue.Writer.TryWrite(address);
        }

        public async ValueTask DisposeAsync()
        {
            _sendQueue.Writer.Complete();
            _closeConnectionQueue.Writer.Complete();

            if (_networkTask is not null)
            {
                await _networkTask;
            }

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

                    while (_closeConnectionQueue.Reader.TryRead(out var addressToClose))
                    {
                        _packetService.CloseConnection(_peer, addressToClose, true);
                    }

                    // Avoid starving packet receiving with sending by
                    // limiting the number of packets sent per batch.
                    while (sendBuffer.CanAdd && _sendQueue.Reader.TryRead(out var packetToSend))
                    {
                        sendBuffer.Add(in packetToSend);
                    }

                    if (sendBuffer.HasBatch)
                    {
                        _packetService.Send(_peer, sendBuffer.GetBatch());

                        sendBuffer.ReleasePending();

                        shouldBackoff = false;
                    }

                    // Poll for incoming packets.
                    var received = _packetService.Receive(_peer);
                    foreach (ref readonly var packet in received)
                    {
                        // Packets that failed to deserialize should not be processed.
                        if (packet.Status != SerializationStatus.Success)
                        {
                            LogMalformedPacket(packet.Sender, packet.Id, packet.Status);
                            packet.Dispose();
                            continue;
                        }

                        // Packet Ids that have been claimed by another network manager should be ignored.
                        if (s_globalClaimedPacketIds.TryGetValue(packet.Id, out var claimBehavior) && !_claimedPacketIds.Contains(packet.Id))
                        {
                            if (claimBehavior == PacketClaimBehavior.Warn)
                            {
                                LogClaimedPacketId(packet.Sender, packet.Id);
                            }

                            packet.Dispose();
                            continue;
                        }

                        _packetProcessor.Enqueue(packet);
                        shouldBackoff = false;
                    }

                    pollingDelayMs = shouldBackoff ? Math.Min(pollingDelayMs * 2, 100) : 1;

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

        [LoggerMessage(Level = LogLevel.Warning, Message = "Client '{Sender}' sent malformed packet with Id '{PacketId}': {Status}")]
        private partial void LogMalformedPacket(NetworkAddress sender, PacketIdentifier packetId, SerializationStatus status);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Ignoring packet with claimed Id '{PacketId}' from '{Sender}'")]
        private partial void LogClaimedPacketId(NetworkAddress sender, PacketIdentifier packetId);

        [LoggerMessage(Level = LogLevel.Critical, Message = "Network failure")]
        private partial void LogNetworkFailure(Exception ex);
    }
}
