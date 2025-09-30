using FluentMigrator.Runner;
using FOMServer.Master.Application.Networking;
using FOMServer.Master.Core.Models;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace FOMServer.Master.Application
{
    public class Server
    {
        private readonly ILogService logService;
        private readonly IMigrationRunner migrationRunner;
        private readonly ServerSettings serverSettings;
        private readonly INetworkService networkService;
        private readonly IServerService serverService;
        private readonly IServiceProvider serviceProvider;

        public Server(
            ILogService logService,
            IMigrationRunner migrationRunner,
            ServerSettings serverSettings,
            INetworkService networkService,
            IServerService serverService,
            IServiceProvider serviceProvider
        )
        {
            this.logService = logService;
            this.migrationRunner = migrationRunner;
            this.serverSettings = serverSettings;
            this.networkService = networkService;
            this.serverService = serverService;
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
            logService.WriteMessage(LogLevel.Info, "Initializing Master Server");

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

            if (!InitializeDatabase())
                return;

            var packetProcessor = new PacketProcessor(
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
            packetProcessor.Start(cts.Token);
            worldNetwork.Start(cts.Token);
            clientNetwork.Start(cts.Token);

            logService.WriteMessage(LogLevel.Info, $"World Port: {serverSettings.WorldPort}");
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
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Initialize the packet sender for communication with world servers.
            var packetSender = serviceProvider.GetRequiredService<WorldPacketSender>();
            packetSender.Initialize(networkManager);

            // Make sure clients can't send packets meant for master<->world communication.
            networkManager.ClaimPacketID(PacketIdentifier.ID_REGISTER_WORLD);

            networkManager.Configure(peer, serverService.Shutdown);
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
