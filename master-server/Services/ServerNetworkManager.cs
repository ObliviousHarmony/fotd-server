using FOMServer.Shared.Enums;
using FOMServer.Shared.Services;
using FOMServer.Shared.Services.FOMNetwork;

namespace FOMServer.Master.Services
{
	internal class ServerNetworkManager : NetworkManager
	{
		private readonly IServerService serverService;

		public ServerNetworkManager(
			ILogService logService,
			IPacketService packetService,
			PacketProcessor packetProcessor,
			IServerService serverService)
			: base(logService, packetService, packetProcessor
		) {
			this.serverService = serverService;
		}

		/// <summary>
		/// Start the FOMNetwork server.
		/// </summary>
		public override void StartPeer()
		{
			base.StartPeer();

			peer = serverService.Startup(61001);
			if (peer == IntPtr.Zero)
				throw new InvalidOperationException("Failed to start server.");

			logService.WriteMessage(LogLevel.Info, "Network Started: 61001");
		}

		/// <summary>
		/// Shuts down the FOMNetwork server.
		/// </summary>
		public override void ShutdownPeer()
		{
			base.ShutdownPeer();

			serverService.Shutdown(peer);

			logService.WriteMessage(LogLevel.Info, "Network Stopped: 61001");
		}
	}
}
