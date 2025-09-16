using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Services.FOMNetwork;
using FOMServer.Shared.Services.Packets;
using System.Threading.Channels;

namespace FOMServer.Shared.Services
{
    /// <summary>
	/// Responsible for sending and receiving packets.
	/// </summary>
	public class NetworkManager : IDisposable
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

		protected IntPtr peer;
		protected Action<IntPtr>? peerShutdown;
		protected readonly ILogService logService;
		private readonly IPacketService packetService;
		private readonly PacketProcessor packetProcessor;
		private readonly Channel<SendPacket> sendQueue;
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

			// Single writer, single reader channel is fine here
			sendQueue = Channel.CreateUnbounded<SendPacket>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = false
			});
		}

		/// <summary>
		/// Configures the network service to use the specified peer.
		/// </summary>
		/// <remarks>
		///	Once the peer has been given to the network manager it should not be used by the caller anymore.
		///	The peerShutdown function will be used for cleanup when the manager is done with the peer.
		/// </remarks>
		/// <param name="peer">The peer to use with the service.</param>
		/// <param name="peerShutdown">A function describing how to shut the peer down when the service is disposed.</param>
        public void ConfigurePeer(IntPtr peer, Action<IntPtr> peerShutdown)
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
			if (networkTask != null)
				return;

			if (peer == IntPtr.Zero)
				throw new InvalidOperationException("Peer is not configured.");

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

		/// <inheritdoc />
		public void Send(
			PacketIdentifier id,
			FOMData data,
			NetworkAddress destination,
			PacketPriority priority,
			PacketReliability reliability,
			byte orderingChannel = 0
		)
		{
			if (peer == IntPtr.Zero)
				throw new InvalidOperationException("Peer is not configured.");

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

		/// <inheritdoc />
		public void Broadcast(
			PacketIdentifier id,
			FOMData data,
			NetworkAddress excludedAddress,
			PacketPriority priority,
			PacketReliability reliability,
			byte orderingChannel = 0
		)
		{
			if (peer == IntPtr.Zero)
				throw new InvalidOperationException("Peer is not configured.");

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
