using FOMServer.Shared.Enums;
using FOMServer.Shared.Handlers;
using FOMServer.Shared.Models;
using System.Reflection;
using System.Threading.Channels;

namespace FOMServer.Shared.Services
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
		private readonly RakNetPacketHandler rakNetPacketHandler;
		private readonly Dictionary<PacketIdentifier, IPacketHandler> handlers;
		private readonly Channel<FOMPacket> packetQueue;
		private readonly List<Task> workers = new();

		private CancellationTokenSource? cts;

		public PacketProcessor(
			ILogService logService,
			RakNetPacketHandler rakNetPacketHandler,
			IEnumerable<IPacketHandler> handlersFromDI
		) {
			this.logService = logService;
			this.rakNetPacketHandler = rakNetPacketHandler;
			this.handlers = handlersFromDI.ToDictionary(h => h.PacketID);

			this.packetQueue = Channel.CreateUnbounded<FOMPacket>(
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
		/// <param name="parentToken">Parent cancellation token.</param>
		/// <param name="workerCount">Number of worker tasks to run.</param>
		public void Start(CancellationToken parentToken, int workerCount = 1)
		{
			if (cts != null)
				return;

			cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

			for (int i = 0; i < workerCount; i++)
			{
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
			catch (OperationCanceledException)
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

				if (TryHandleRakNetPacket(packet))
					continue;

				if (packet.ID >= PacketIdentifier.ID_CONNECTION_REQUEST_ACCEPTED && packet.ID <= PacketIdentifier.ID_CONNECTION_BANNED)
				{
					// These are RakNet internal packets that we don't handle here.
					continue;
				}

				if (handlers.TryGetValue(packet.ID, out var handler))
					handler.Handle(packet);
				else
					OnUnhandledPacket(packet);
			}
		}

		/// <summary>
		/// Checks the packet's ID and handles it if it is one of the RakNet packets we care about.
		/// </summary>
		/// <param name="packet">The packet to check.</param>
		/// <returns>True if the packet is a RakNet packet to handle, otherwise false.</returns>
		private bool TryHandleRakNetPacket(FOMPacket packet)
		{
			// Only handle the RakNet packets defined in PacketIdentifier.
			switch (packet.ID)
			{
				case PacketIdentifier.ID_CONNECTION_REQUEST_ACCEPTED:
				case PacketIdentifier.ID_CONNECTION_ATTEMPT_FAILED:
				case PacketIdentifier.ID_ALREADY_CONNECTED:
				case PacketIdentifier.ID_NEW_INCOMING_CONNECTION:
				case PacketIdentifier.ID_NO_FREE_INCOMING_CONNECTIONS:
				case PacketIdentifier.ID_DISCONNECTION_NOTIFICATION:
				case PacketIdentifier.ID_CONNECTION_LOST:
				case PacketIdentifier.ID_RSA_PUBLIC_KEY_MISMATCH:
				case PacketIdentifier.ID_CONNECTION_BANNED:
				case PacketIdentifier.ID_INVALID_PASSWORD:
				case PacketIdentifier.ID_MODIFIED_PACKET:
					break;
				default:
					return false;
			}

			rakNetPacketHandler.Handle(packet.ID, packet.Sender);
			return true;
		}

		/// <summary>
		/// When a packet has no handler defined, this function will be called so it can be dealt with.
		/// </summary>
		/// <param name="packet">The packet to handle.</param>
		private void OnUnhandledPacket(FOMPacket packet)
		{
			this.logService.WriteMessage(LogLevel.Error, $"No handler for packet {packet.ID} from {packet.Sender}");
		}

		public void Dispose()
		{
			StopAsync().GetAwaiter().GetResult();
		}
	}
}
