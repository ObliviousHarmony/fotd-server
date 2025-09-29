using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Infrastructure.Services;
using System.Threading.Channels;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
    /// Service for processing incoming packets.
    ///
    /// For performance, this maintains a number of worker tasks that process the packets
    /// concurrently.
    /// </summary>
    public class PacketProcessor : IDisposable
    {
        private readonly ILogService logService;
        private readonly Dictionary<PacketIdentifier, IPacketHandler> handlers;
        private readonly Channel<FOMPacket> packetQueue;
        private readonly List<Task> workers = [];

        private CancellationTokenSource? cts;

        public PacketProcessor(ILogService logService, IEnumerable<IPacketHandler> handlers)
        {
            this.logService = logService;
            this.handlers = handlers.ToDictionary(h => h.PacketID);

            packetQueue = Channel.CreateUnbounded<FOMPacket>(
                new UnboundedChannelOptions
                {
                    SingleReader = false,
                    SingleWriter = true
                }
            );
        }

        /// <summary>
        /// Enqueue a packet for processing.
        /// </summary>
        public void Enqueue(in FOMPacket packet)
        {
            packetQueue.Writer.TryWrite(packet);
        }

        /// <summary>
        /// Start worker threads to process packets.
        /// </summary>
        public void Start(CancellationToken parentToken, int workerCount = 1)
        {
            if (cts != null)
                return;

            cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

            for (int i = 0; i < workerCount; i++)
            {
                // Use a dedicated thread for each worker because new packets
                // will consistently be arriving and needing to be handled.
                var task = Task.Factory.StartNew(
                    async () => await WorkerLoopAsync(cts.Token),
                    cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                ).Unwrap();

                workers.Add(task);
            }
        }

        /// <summary>
        /// Stop processing gracefully.
        /// </summary>
        public async Task StopAsync()
        {
            if (cts == null)
                return;

            cts.Cancel();
            packetQueue.Writer.Complete();

            try
            {
                await Task.WhenAll(workers);
            }
            catch (Exception)
            {
            }

            cts.Dispose();
            cts = null;
            workers.Clear();
        }

        /// <summary>
        /// The looping function for each worker task.
        /// </summary>
        private async Task WorkerLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                FOMPacket packet;

                try
                {
                    packet = await packetQueue.Reader.ReadAsync(ct);
                }
                catch (ChannelClosedException)
                {
                    break;
                }

                try
                {
                    if (handlers.TryGetValue(packet.ID, out var handler))
                        handler.Handle(packet);
                    else
                        OnUnhandledPacket(packet);
                }
                catch (Exception ex)
                {
                    // Letting unhandled exceptions prevent further packet processing
                    // would silently break break the server, so log and continue.
                    logService.WritePacketException(packet, ex);
                    continue;
                }
            }
        }

        /// <summary>
        /// When a packet has no handler defined, this function will be called so it can be dealt with.
        /// </summary>
        private void OnUnhandledPacket(FOMPacket packet)
        {
            // Any unhandled internal packets should be ignored.
            if (packet.ID < PacketIdentifier.ID_FOM_PACKET_START)
                return;

            throw new NotSupportedException("Missing Packet Handler");
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();

            GC.SuppressFinalize(this);
        }
    }
}
