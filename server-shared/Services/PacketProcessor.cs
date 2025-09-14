using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
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
		private readonly Channel<FOMPacket> packetQueue;
		private readonly List<Task> workers = new();
		private readonly Dictionary<PacketIdentifier, IPacketHandler> handlers;

		private CancellationTokenSource? cts;

		public PacketProcessor(
			IEnumerable<IPacketHandler> handlersFromDI)
		{
			this.packetQueue = Channel.CreateUnbounded<FOMPacket>(
				new UnboundedChannelOptions
				{
					SingleReader = false,
					SingleWriter = true
				}
			);
			this.handlers = handlersFromDI.ToDictionary(h => h.PacketID);
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

				if (handlers.TryGetValue(packet.ID, out var handler))
					handler.Handle(packet);
				else
					OnUnhandledPacket(packet);
			}
		}

		/// <summary>
		/// When a packet has no handler defined, this function will be called so it can be dealt with.
		/// </summary>
		/// <param name="packet">The packet to handle.</param>
		private void OnUnhandledPacket(FOMPacket packet)
		{
		}

		public void Dispose()
		{
			StopAsync().GetAwaiter().GetResult();
		}
	}
}
