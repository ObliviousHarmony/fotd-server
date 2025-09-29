using FOMServer.World.Application.Networking;
using FOMServer.World.Core.Models;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.World.Application
{
    public class Server
    {
        private readonly ILogService logService;
        private readonly ServerSettings serverSettings;
        private readonly INetworkService networkService;
        private readonly IServerService serverService;
        private readonly IClientService clientService;
        private readonly MasterPacketSender masterPacketSender;
        private readonly ClientPacketSender clientPacketSender;
        private readonly IServiceProvider serviceProvider;

        public Server(
            ILogService logService,
            ServerSettings serverSettings,
            INetworkService networkService,
            IServerService serverService,
            IClientService clientService,
            ClientPacketSender clientPacketSender,
            MasterPacketSender masterPacketSender,
            IServiceProvider serviceProvider
        )
        {
            this.logService = logService;
            this.serverSettings = serverSettings;
            this.networkService = networkService;
            this.serverService = serverService;
            this.clientService = clientService;
            this.clientPacketSender = clientPacketSender;
            this.masterPacketSender = masterPacketSender;
            this.serviceProvider = serviceProvider;
        }

        public void Run()
        {
            var cts = new CancellationTokenSource();

            logService.WriteMessage(LogLevel.Info, "Starting World Server...");
            logService.WriteMessage(LogLevel.Info, "Press Ctrl+C for shutdown.");

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

            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            networkService.ValidateFOMPacket();

            var packetProcessor = new PacketProcessor(
               logService,
               serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            if (!ConnectToMaster(cts.Token, packetProcessor))
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to connect to the master server.");
                return;
            }

            if (!StartClientNetwork(cts.Token, packetProcessor))
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to start the client network.");
                return;
            }

            packetProcessor.Start(cts.Token);

            try
            {
                WaitHandle.WaitAny(new[] { cts.Token.WaitHandle });
            }
            catch (OperationCanceledException)
            {
            }
        }

        private bool ConnectToMaster(CancellationToken ctParent, PacketProcessor packetProcessor)
        {
            var peer = clientService.Connect(serverSettings.MasterServer, serverSettings.MasterServerPort);
            if (peer == IntPtr.Zero)
                return false;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            masterPacketSender.Initialize(networkManager);
            networkManager.Start(ctParent, peer, serverService.Shutdown);

            logService.WriteMessage(LogLevel.Info, $"Master Server: {serverSettings.MasterServer}:{serverSettings.MasterServerPort}");

            return true;
        }

        private bool StartClientNetwork(CancellationToken ctParent, PacketProcessor packetProcessor)
        {
            var peer = serverService.Startup(serverSettings.Port);
            if (peer == IntPtr.Zero)
                return false;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            clientPacketSender.Initialize(networkManager);
            networkManager.Start(ctParent, peer, serverService.Shutdown);

            logService.WriteMessage(LogLevel.Info, $"Port: {serverSettings.Port}");

            return true;
        }
    }
}
