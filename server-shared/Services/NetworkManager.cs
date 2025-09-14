using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Services.FOMNetwork;
using System.Threading.Channels;

namespace FOMServer.Shared.Services
{
    /// <summary>
	/// Responsible for sending and receiving packets.
	/// </summary>
	public class NetworkManager : ISendPackets, IDisposable
	{
		/// <summary>
		/// A buffer for holding packets to send via the API.
		/// </summary>
		/// <remarks>
		/// Using this buffer allows us to avoid allocating a new array.
		/// We can pin the memory of the pre-allocated buffer and then
		/// pass that without having to do anything special.
		/// </remarks>
		private static readonly SendPacket[] SendBuffer = new SendPacket[IPacketService.MaxBufferedPackets];

		private readonly IPacketService packetService;
		private readonly PacketProcessor packetProcessor;
		private readonly Channel<SendPacket> sendQueue;
		private Task? networkTask;
		private CancellationTokenSource? cts;

		public NetworkManager(IPacketService packetService, PacketProcessor packetProcessor)
		{
			this.packetService = packetService;
			this.packetProcessor = packetProcessor;

			// Single writer, single reader channel is fine here
			sendQueue = Channel.CreateUnbounded<SendPacket>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = false
			});
		}

		public void Send(
			PacketIdentifier id,
			FOMData data,
			NetworkAddress destination,
			PacketPriority priority,
			PacketReliability reliability,
			byte orderingChannel = 0)
		{
			SendPacket packet = new()
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
			FOMData data,
			NetworkAddress excludedAddress,
			PacketPriority priority,
			PacketReliability reliability,
			byte orderingChannel = 0)
		{
			SendPacket packet = new()
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

		/// <summary>
		/// Starts the network manager loop.
		/// </summary>
		public void Start(CancellationToken parentToken)
		{
			if (networkTask != null)
				return;

			cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

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
			// Temporary variable to allow for building!
			IntPtr peer = IntPtr.Zero;

			while (!ct.IsCancellationRequested)
			{
				// Avoid starving packet receiving with sending by
				// limiting the number of packets sent per batch.
				int numToSend = 0;
				while (numToSend < IPacketService.MaxBufferedPackets && sendQueue.Reader.TryRead(out var packetToSend))
					SendBuffer[numToSend++] = packetToSend;

				if (numToSend > 0)
					packetService.Send(peer, SendBuffer.AsSpan(0, numToSend));

				var received = packetService.Receive(peer);
				foreach (ref readonly var packet in received)
					packetProcessor.Enqueue(packet);

				await Task.Yield();
			}
		}

		public void Dispose()
		{
			StopAsync().GetAwaiter().GetResult();
		}
	}
}
