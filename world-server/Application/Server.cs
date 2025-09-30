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
        private readonly IServiceProvider serviceProvider;

        public Server(
            ILogService logService,
            ServerSettings serverSettings,
            INetworkService networkService,
            IServerService serverService,
            IClientService clientService,
            IServiceProvider serviceProvider
        )
        {
            this.logService = logService;
            this.serverSettings = serverSettings;
            this.networkService = networkService;
            this.serverService = serverService;
            this.clientService = clientService;
            this.serviceProvider = serviceProvider;
        }

        public void Run()
        {
            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            networkService.ValidateFOMPacket();

            var cts = new CancellationTokenSource();

            logService.WriteMessage(LogLevel.Info, "------------------------------------------------");
            logService.WriteMessage(LogLevel.Info, "Initializing World Server");

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

            var packetProcessor = new PacketProcessor(
               logService,
               serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            var masterNetwork = ConnectToMasterNetwork(packetProcessor);
            if (masterNetwork == null)
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to connect to the master server.");
                return;
            }

            var clientNetwork = CreateClientNetwork(packetProcessor);
            if (clientNetwork == null)
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to create the client network.");
                return;
            }

            // The server is now ready to start processing packets.
            packetProcessor.Start(cts.Token);
            masterNetwork.Start(cts.Token);
            clientNetwork.Start(cts.Token);

            logService.WriteMessage(LogLevel.Info, $"Master Server: {serverSettings.MasterServer}:{serverSettings.MasterServerPort}");
            logService.WriteMessage(LogLevel.Info, $"Client Port: {serverSettings.ClientPort}");
            logService.WriteMessage(LogLevel.Info, "------------------------------------------------");

            try
            {
                WaitHandle.WaitAny(new[] { cts.Token.WaitHandle });
            }
            catch (OperationCanceledException)
            {
            }
        }

        private NetworkManager? ConnectToMasterNetwork(PacketProcessor packetProcessor)
        {
            var peer = clientService.Connect(serverSettings.MasterServer, serverSettings.MasterServerPort);
            if (peer == IntPtr.Zero)
                return null;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Initialize the packet sender for communication with the master server.
            var packetSender = serviceProvider.GetRequiredService<MasterPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, clientService.Disconnect);
            return networkManager;
        }

        private NetworkManager? CreateClientNetwork(PacketProcessor packetProcessor)
        {
            var peer = serverService.Startup(serverSettings.ClientPort);
            if (peer == IntPtr.Zero)
                return null;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Initialize the packet sender for communication with clients.
            var packetSender = serviceProvider.GetRequiredService<ClientPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, serverService.Shutdown);
            return networkManager;
        }
    }
}
