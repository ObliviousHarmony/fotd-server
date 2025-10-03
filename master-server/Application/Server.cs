using FluentMigrator.Runner;
using FOMServer.Master.Application.Networking;
using FOMServer.Master.Core;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Networking;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace FOMServer.Master.Application
{
    public class Server
    {
        private readonly ILogService logService;
        private readonly IShutdownManager shutdownManager;
        private readonly IMigrationRunner migrationRunner;
        private readonly ServerSettings serverSettings;
        private readonly INetworkService networkService;
        private readonly IServerService serverService;
        private readonly IServiceProvider serviceProvider;

        public Server(
            ILogService logService,
            IShutdownManager shutdownManager,
            IMigrationRunner migrationRunner,
            ServerSettings serverSettings,
            INetworkService networkService,
            IServerService serverService,
            IServiceProvider serviceProvider
        )
        {
            this.logService = logService;
            this.shutdownManager = shutdownManager;
            this.migrationRunner = migrationRunner;
            this.serverSettings = serverSettings;
            this.networkService = networkService;
            this.serverService = serverService;
            this.serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            networkService.ValidateFOMPacket();

            logService.WriteMessage(LogLevel.Info, "------------------------------------------------");
            logService.WriteMessage(LogLevel.Info, "Initializing Master Server");

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

            if (!InitializeDatabase())
                return;

            var packetProcessor = new PacketProcessor(
                serviceProvider.GetRequiredService<IShutdownManager>(),
                logService,
                serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            var worldNetwork = CreateWorldServerNetwork(packetProcessor);
            if (worldNetwork == null)
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to create the world server network.");
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
            worldNetwork.Start();
            clientNetwork.Start();

            logService.WriteMessage(LogLevel.Info, $"World Port: {serverSettings.WorldPort}");
            logService.WriteMessage(LogLevel.Info, $"Client Port: {serverSettings.ClientPort}");
            logService.WriteMessage(LogLevel.Info, "------------------------------------------------");

            await shutdownManager.Stopped;
            logService.WriteMessage(LogLevel.Info, "Shutdown Complete");
        }

        private bool InitializeDatabase()
        {
            try
            {
                migrationRunner.MigrateUp();
            }
            catch (MySqlException)
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to connect to the database. Please check your connection settings.");
                return false;
            }
            catch (Exception ex)
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to apply database migrations.");
                logService.WriteException(ex);
                return false;
            }

            return true;
        }

        private NetworkManager? CreateWorldServerNetwork(PacketProcessor packetProcessor)
        {
            var peer = serverService.Startup(serverSettings.WorldPort);
            if (peer == IntPtr.Zero)
                return null;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IShutdownManager>(),
                serviceProvider.GetRequiredService<ILogService>(),
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Make sure clients can't send packets meant for master<->world communication.
            networkManager.ClaimPacketID(PacketIdentifier.ID_REGISTER_WORLD);

            // Initialize the packet sender for communication with world servers.
            var packetSender = serviceProvider.GetRequiredService<WorldPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, serverService.Shutdown);
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
