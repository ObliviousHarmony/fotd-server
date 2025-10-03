using Microsoft.Extensions.DependencyInjection;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core.Networking;
using FOMServer.World.Application.Networking;
using FOMServer.World.Core;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core;

namespace FOMServer.World.Application
{
    public class Server
    {
        private readonly ILogService logService;
        private readonly IShutdownManager shutdownManager;
        private readonly ServerSettings serverSettings;
        private readonly INetworkService networkService;
        private readonly IServerService serverService;
        private readonly IClientService clientService;
        private readonly IServiceProvider serviceProvider;

        public Server(
            ILogService logService,
            IShutdownManager shutdownManager,
            ServerSettings serverSettings,
            INetworkService networkService,
            IServerService serverService,
            IClientService clientService,
            IServiceProvider serviceProvider
        )
        {
            this.logService = logService;
            this.shutdownManager = shutdownManager;
            this.serverSettings = serverSettings;
            this.networkService = networkService;
            this.serverService = serverService;
            this.clientService = clientService;
            this.serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            networkService.ValidateFOMPacket();

            logService.WriteMessage(LogLevel.Info, "------------------------------------------------");
            logService.WriteMessage(LogLevel.Info, "Initializing World Server");

            Console.CancelKeyPress += (sender, e) =>
            {
                logService.WriteMessage(LogLevel.Info, "Stopping Server...");

                e.Cancel = true;
                shutdownManager.Shutdown();
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                shutdownManager.Shutdown();
            };

            var packetProcessor = new PacketProcessor(
                serviceProvider.GetRequiredService<IShutdownManager>(),
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
            packetProcessor.Start();
            masterNetwork.Start();
            clientNetwork.Start();

            logService.WriteMessage(LogLevel.Info, $"Master Server: {serverSettings.MasterServerAddress}:{serverSettings.MasterServerPort}");
            logService.WriteMessage(LogLevel.Info, $"Client Port: {serverSettings.ClientPort}");
            logService.WriteMessage(LogLevel.Info, "------------------------------------------------");

            await shutdownManager.Stopped;
            logService.WriteMessage(LogLevel.Info, "Shutdown Complete");
        }

        private NetworkManager? ConnectToMasterNetwork(PacketProcessor packetProcessor)
        {
            var peer = clientService.Connect(serverSettings.MasterServerAddress, serverSettings.MasterServerPort);
            if (peer == IntPtr.Zero)
                return null;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IShutdownManager>(),
                serviceProvider.GetRequiredService<ILogService>(),
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Initialize the packet sender for communication with the master server.
            var packetSender = serviceProvider.GetRequiredService<MasterPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, clientService.Disconnect);

            // Register this world server with the master server.
            packetSender.Send(
                PacketIdentifier.ID_REGISTER_WORLD,
                new FOMDataUnion
                {
                    registerWorld = new RegisterWorld
                    {
                        WorldID = serverSettings.WorldID,
                        Port = serverSettings.ClientPort,
                        Address = "127.0.0.1"
                    }
                },
                PacketPriority.HIGH_PRIORITY,
                PacketReliability.RELIABLE
            );

            return networkManager;
        }

        private NetworkManager? CreateClientNetwork(PacketProcessor packetProcessor)
        {
            var peer = serverService.Startup(serverSettings.ClientPort);
            if (peer == IntPtr.Zero)
                return null;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IShutdownManager>(),
                serviceProvider.GetRequiredService<ILogService>(),
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
