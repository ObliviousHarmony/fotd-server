using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Infrastructure.Services;
using System.Threading.Channels;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
	/// Responsible for sending and receiving packets.
	/// </summary>
	public class NetworkManager : IPacketSender, IDisposable
    {
        /// <summary>
        /// Individual network managers can "claim" packet IDs so that they
        /// exclusively handle them. When another network manager receives
        /// a packet with a claimed ID, it will ignore it.
        /// </summary>
        private static readonly HashSet<PacketIdentifier> globalClaimedPacketIDs = new HashSet<PacketIdentifier>();

        /// <summary>
        /// Packet ID claims are not using a thread-safe collection for
        /// performance reasons. Claims should be done during
        /// initialization and be prevented once the first
        /// network manager has started.
        /// </summary>
        private static bool canClaimPacketIDs = true;

        /// <summary>
        /// A buffer for holding packets to send via the API.
        /// </summary>
        /// <remarks>
        /// Using this buffer allows us to avoid allocating a new array.
        /// We can pin the memory of the pre-allocated buffer and then
        /// pass that without having to do anything special.
        /// </remarks>
        private readonly SendPacket[] sendBuffer = new SendPacket[IPacketService.MaxBufferedPackets];

        private IntPtr peer;
        private Action<IntPtr>? peerShutdown;
        private readonly ILogService logService;
        private readonly IPacketService packetService;
        private readonly PacketProcessor packetProcessor;
        private readonly Channel<SendPacket> sendQueue;
        private readonly HashSet<PacketIdentifier> claimedPacketIDs;
        private Task? networkTask;
        private CancellationTokenSource? cts;

        public NetworkManager(
            ILogService logService,
            IPacketService packetService,
            PacketProcessor packetProcessor
        )
        {
            this.peer = IntPtr.Zero;
            this.peerShutdown = null;
            this.logService = logService;
            this.packetService = packetService;
            this.packetProcessor = packetProcessor;
            this.claimedPacketIDs = new HashSet<PacketIdentifier>();

            // Single writer, single reader channel is fine here
            this.sendQueue = Channel.CreateUnbounded<SendPacket>(new UnboundedChannelOptions
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
            if (!canClaimPacketIDs)
                throw new InvalidOperationException("Cannot claim packet IDs after a network manager has started.");

            if (globalClaimedPacketIDs.Contains(id))
                throw new InvalidOperationException($"Packet ID {id} is already claimed by another network manager.");

            globalClaimedPacketIDs.Add(id);
            claimedPacketIDs.Add(id);
        }

        /// <summary>
        /// Configures the network manager with the necessary parameters.
        /// </summary>
        public void Configure(IntPtr peer, Action<IntPtr> peerShutdown)
        {
            if (this.peer != IntPtr.Zero)
                throw new InvalidOperationException("Peer is already configured.");

            this.peer = peer;
            this.peerShutdown = peerShutdown;
        }

        /// <summary>
        /// Starts the network manager loop.
        /// </summary>
        public void Start(CancellationToken parentToken)
        {
            if (peer == IntPtr.Zero)
                throw new InvalidOperationException("Peer must be configured.");

            if (networkTask != null)
                return;

            // Once a network manager has started, no more claims can be made.
            canClaimPacketIDs = false;

            cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

            // Use a dedicated thread for this task because we need to
            // keep polling the network library to maximize throughput.
            networkTask = Task.Factory.StartNew(
                async () => await NetworkLoopAsync(cts.Token),
                cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            ).Unwrap();
        }

        /// <summary>
        /// Stops the network manager gracefully.
        /// </summary>
        public async Task StopAsync()
        {
            if (networkTask == null)
                return;

            cts?.Cancel();
            sendQueue.Writer.Complete();

            try
            {
                await networkTask;
            }
            catch (OperationCanceledException)
            {
            }

            networkTask = null;
            cts?.Dispose();
            cts = null;
        }

        private async Task NetworkLoopAsync(CancellationToken ct)
        {
            int pollingDelayMs = 1;
            while (!ct.IsCancellationRequested)
            {
                // Avoid starving packet receiving with sending by
                // limiting the number of packets sent per batch.
                int numToSend = 0;
                while (numToSend < IPacketService.MaxBufferedPackets && sendQueue.Reader.TryRead(out var packetToSend))
                    sendBuffer[numToSend++] = packetToSend;

                if (numToSend > 0)
                    packetService.Send(peer, sendBuffer.AsSpan(0, numToSend));

                var received = packetService.Receive(peer);
                foreach (ref readonly var packet in received)
                {
                    // Packet IDs that have been claimed by another network manager should be ignored.
                    if (globalClaimedPacketIDs.Contains(packet.ID) && !claimedPacketIDs.Contains(packet.ID))
                    {
                        logService.WriteMessage(LogLevel.Error, $"Client {packet.Sender} sent packet with claimed ID {packet.ID}, ignoring.");
                        continue;
                    }

                    packetProcessor.Enqueue(packet);
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

        public void Send(
            PacketIdentifier id,
            FOMDataUnion data,
            NetworkAddress destination,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0
        )
        {
            if (peer == IntPtr.Zero)
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

            sendQueue.Writer.TryWrite(packet);
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
            if (peer == IntPtr.Zero)
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

            sendQueue.Writer.TryWrite(packet);
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();

            if (peerShutdown != null && peer != IntPtr.Zero)
            {
                peerShutdown(peer);
                peerShutdown = null;
            }

            if (peer != IntPtr.Zero)
            {
                peer = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
        }
    }
}
