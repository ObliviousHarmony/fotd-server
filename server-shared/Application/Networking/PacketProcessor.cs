using System.Reflection;
using System.Threading.Channels;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
    /// Service for processing incoming packets.
    ///
    /// For performance, this maintains a number of worker tasks that process the packets
    /// concurrently.
    /// </summary>
    public class PacketProcessor
    {
        private readonly IShutdownManager _shutdownManager;
        private readonly ILogService _logService;
        private readonly Dictionary<PacketIdentifier, IPacketHandler> _handlers;
        private readonly Channel<Packet> _packetQueue;
        private readonly List<Task> _workers = [];

        private CancellationTokenSource? _cts;

        public PacketProcessor(IShutdownManager shutdownManager, ILogService logService, IEnumerable<IPacketHandler> handlers)
        {
            _shutdownManager = shutdownManager;
            _logService = logService;

            // Build the map by extracting the PacketIdentifier from each handler's generic packet struct argument.
            _handlers = handlers.ToDictionary(h => {
                var handlerType = h.GetType();

                if (!Attribute.IsDefined(handlerType, typeof(PacketHandlerAttribute)))
                    throw new InvalidOperationException($"Handler type {handlerType.Name} is missing [PacketHandler]");

                var baseType = handlerType.BaseType;
                if (baseType == null || !baseType.IsGenericType)
                    throw new InvalidOperationException($"Handler {handlerType.Name} does not derive from BasePacketHandler<T>.");

                var packetType = baseType.GetGenericArguments()[0];
                var packetIDAttr = packetType.GetCustomAttribute<PacketIDAttribute>();
                if (packetIDAttr == null)
                    throw new InvalidOperationException($"Packet type {packetType.Name} is missing [PacketID].");

                return packetIDAttr.ID;
            });

            _packetQueue = Channel.CreateUnbounded<Packet>(
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
        public void Enqueue(in Packet packet)
        {
            _packetQueue.Writer.TryWrite(packet);
        }

        /// <summary>
        /// Start worker threads to process packets.
        /// </summary>
        public void Start(int workerCount = 1)
        {
            if (_cts != null)
                return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownManager.Token);

            for (int i = 0; i < workerCount; i++)
            {
                // Use a dedicated thread for each worker because new packets
                // will consistently be arriving and needing to be handled.
                var task = Task.Factory.StartNew(
                    async () => await WorkerLoopAsync(_cts.Token),
                    _cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                ).Unwrap();

                _workers.Add(task);
            }

            // Make sure the shutdown manager waits for all of the packet workers to complete.
            _shutdownManager.TrackTask(Task.WhenAll(_workers));
        }

        /// <summary>
        /// The looping function for each worker task.
        /// </summary>
        private async Task WorkerLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Packet packet;

                try
                {
                    packet = await _packetQueue.Reader.ReadAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ChannelClosedException)
                {
                    break;
                }

                try
                {
                    if (_handlers.TryGetValue(packet.ID, out var handler))
                        handler.Handle(packet);
                    else
                        OnUnhandledPacket(packet);
                }
                catch (Exception ex)
                {
                    // Letting unhandled exceptions prevent further packet processing
                    // would silently break break the server, so log and continue.
                    _logService.WritePacketException(packet, ex);
                    continue;
                }
            }

            // We intentionally do not drain the queue here because it
            // might cause race conditions with other threads that
            // are shutting down by mutating shared state.
        }

        /// <summary>
        /// When a packet has no handler defined, this function will be called so it can be dealt with.
        /// </summary>
        private void OnUnhandledPacket(Packet packet)
        {
            // Any unhandled internal packets should be ignored.
            if (packet.ID < PacketIdentifier.ID_FOM_PACKET_START)
                return;

            throw new NotSupportedException("Missing Packet Handler");
        }
    }
}
