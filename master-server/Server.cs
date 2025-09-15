using FOMServer.Shared.Enums;
using FOMServer.Shared.Services;
using FOMServer.Master.Services;

namespace FOMServer.Master
{
	internal class Server
	{
		private readonly LogService logService;
		private readonly ServerNetworkManager networkManager;
		private readonly PacketProcessor packetProcessor;

		public Server(
			LogService logService,
			ServerNetworkManager networkManager,
			PacketProcessor packetProcessor
		)
		{
			this.logService = logService;
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

			// Start the network peer so we can accept connections.
			networkManager.StartPeer();

			// Start all of our services so they will spin up their background tasks.
			logService.Start(cts.Token);
			networkManager.Start(cts.Token);
			packetProcessor.Start(cts.Token);

			// Make sure that we can gracefully handle shutdown.
			Console.CancelKeyPress += (sender, e) =>
			{
				this.logService.WriteMessage(LogLevel.Info, "Stopping Server...");

				e.Cancel = true; // don't kill process immediately
				cts.Cancel();
			};
			AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
			{
				this.logService.WriteMessage(LogLevel.Info, "Stopping Server...");
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
