using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Infrastructure.Services;
using FOMServer.Shared.Application.Networking;
using MySqlConnector;
using FOMServer.World.Core.Models;
using FOMServer.World.Application.Networking;

namespace FOMServer.World.Application
{
    internal class Server
    {
        private readonly ServerSettings serverSettings;
        private readonly LogService logService;
        private readonly IServerService serverService;
        private readonly INetworkService networkService;
        private readonly NetworkManager networkManager;
        private readonly MasterConnectionManager masterConnectionManager;
        private readonly PacketProcessor packetProcessor;

        public Server(
            ServerSettings serverSettings,
            LogService logService,
            IServerService serverService,
            INetworkService networkService,
            NetworkManager networkManager,
            MasterConnectionManager masterConnectionManager,
            PacketProcessor packetProcessor
        )
        {
            this.serverSettings = serverSettings;
            this.logService = logService;
            this.serverService = serverService;
            this.networkService = networkService;
            this.networkManager = networkManager;
            this.masterConnectionManager = masterConnectionManager;
            this.packetProcessor = packetProcessor;
        }

        /// <summary>
        /// Run the server until cancelled.
        /// </summary>
        public void Run()
        {
            var cts = new CancellationTokenSource();

            // Start the logging service first so we can log everything else.
            logService.Start(cts.Token);

            logService.WriteMessage(LogLevel.Info, $"Starting World ({serverSettings.WorldID}) Server...");
            logService.WriteMessage(LogLevel.Info, "Press Ctrl+C for shutdown.");

            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            networkService.ValidateFOMPacket();

            // Start the server peer so we can accept connections.
            var peer = serverService.Startup(serverSettings.Port);
            if (peer == IntPtr.Zero)
                throw new InvalidOperationException("Failed to start server.");
            networkManager.ConfigurePeer(peer, serverService.Shutdown);

            logService.WriteMessage(LogLevel.Info, $"Network Started: {serverSettings.Port}");

            // Start all of our services so they will spin up their background tasks.
            networkManager.Start(cts.Token);
            packetProcessor.Start(cts.Token);

            // Make sure that we can gracefully handle shutdown.
            Console.CancelKeyPress += (sender, e) =>
            {
                logService.WriteMessage(LogLevel.Info, "Stopping Server...");

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
            catch (OperationCanceledException)
            {
            }
        }
    }
}
