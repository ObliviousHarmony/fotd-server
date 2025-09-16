using FOMServer.Shared.Enums;
using FOMServer.Shared.Services;
using FOMServer.Shared.Services.FOMNetwork;
using FOMServer.Shared.Services.Packets;

namespace FOMServer.Master
{
	internal class Server
	{
		private readonly LogService logService;
		private readonly IServerService serverService;
		private readonly INetworkService networkService;
		private readonly NetworkManager networkManager;
		private readonly PacketProcessor packetProcessor;

		public Server(
			LogService logService,
			IServerService serverService,
			INetworkService networkService,
			NetworkManager networkManager,
			PacketProcessor packetProcessor
		)
		{
			this.logService = logService;
			this.serverService = serverService;
			this.networkService = networkService;
			this.networkManager = networkManager;
			this.packetProcessor = packetProcessor;
		}

		/// <summary>
		/// Run the server until cancelled.
		/// </summary>
		public void Run()
		{
			CancellationTokenSource cts = new CancellationTokenSource();

			this.logService.WriteMessage(LogLevel.Info, "Starting Server...");
			this.logService.WriteMessage(LogLevel.Info, "Press Ctrl+C for shutdown.");

			// We need to make sure our packet structs are all blittable and match the C++ side.
			// This is critical to ensure that we don't have memory corruption and don't
			// require expensive marshalling of data between managed and unmanaged code.
			networkService.ValidateFOMPacket();

			// Start the network peer so we can accept connections.
			IntPtr peer = serverService.Startup(61001);
			if (peer == IntPtr.Zero)
				throw new InvalidOperationException("Failed to start server.");
			networkManager.ConfigurePeer(peer, serverService.Shutdown);

			logService.WriteMessage(LogLevel.Info, "Network Started: 61001");

			// Start all of our services so they will spin up their background tasks.
			logService.Start(cts.Token);
			networkManager.Start(cts.Token);
			packetProcessor.Start(cts.Token);

			// Make sure that we can gracefully handle shutdown.
			Console.CancelKeyPress += (sender, e) =>
			{
				this.logService.WriteMessage(LogLevel.Info, "Stopping Server...");

				e.Cancel = true;
				cts.Cancel();
			};
			AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
			{
				cts.Cancel();
			};

			try
			{
				WaitHandle.WaitAny(new[] { cts.Token.WaitHandle });
			}
			catch (OperationCanceledException) { }
		}
	}
}
