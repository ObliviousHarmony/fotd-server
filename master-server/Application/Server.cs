using FluentMigrator.Runner;
using FOMServer.Master.Application.Networking;
using FOMServer.Master.Core;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace FOMServer.Master.Application
{
    public class Server
    {
        private readonly ILogService _logService;
        private readonly IShutdownManager _shutdownManager;
        private readonly IMigrationRunner _migrationRunner;
        private readonly INetworkService _networkService;
        private readonly IServerService _serverService;
        private readonly IServiceProvider _serviceProvider;

        public Server(
            ILogService logService,
            IShutdownManager shutdownManager,
            IMigrationRunner migrationRunner,
            INetworkService networkService,
            IServerService serverService,
            IServiceProvider serviceProvider
        )
        {
            _logService = logService;
            _shutdownManager = shutdownManager;
            _migrationRunner = migrationRunner;
            _networkService = networkService;
            _serverService = serverService;
            _serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            _networkService.ValidatePacketStructs();

            _logService.WriteMessage(LogLevel.Info, "------------------------------------------------");
            _logService.WriteMessage(LogLevel.Info, "Initializing Master Server");

            Console.CancelKeyPress += (sender, e) =>
            {
                _logService.WriteMessage(LogLevel.Info, "Stopping Server...");

                e.Cancel = true;
                _shutdownManager.Shutdown();
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                _shutdownManager.Shutdown();
            };

            if (!InitializeDatabase())
                return;

            var packetProcessor = new PacketProcessor(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _logService,
                _serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            var worldNetwork = CreateWorldServerNetwork(packetProcessor);
            if (worldNetwork == null)
            {
                _logService.WriteMessage(LogLevel.Critical, "Failed to create the world server network.");
                return;
            }

            var clientNetwork = CreateClientNetwork(packetProcessor);
            if (clientNetwork == null)
            {
                _logService.WriteMessage(LogLevel.Critical, "Failed to create the client network.");
                return;
            }

            // The server is now ready to start processing packets.
            packetProcessor.Start();
            worldNetwork.Start();
            clientNetwork.Start();

            foreach (var startable in _serviceProvider.GetServices<IServerStartable>())
                startable.Start();

            _logService.WriteMessage(LogLevel.Info, "------------------------------------------------");

            await _shutdownManager.Stopped;
            _logService.WriteMessage(LogLevel.Info, "Shutdown Complete");
        }

        private bool InitializeDatabase()
        {
            try
            {
                _migrationRunner.MigrateUp();
            }
            catch (MySqlException)
            {
                _logService.WriteMessage(LogLevel.Critical, "Failed to connect to the database. Please check your connection settings.");
                return false;
            }
            catch (Exception ex)
            {
                _logService.WriteMessage(LogLevel.Critical, "Failed to apply database migrations.");
                _logService.WriteException(ex);
                return false;
            }

            return true;
        }

        private NetworkManager? CreateWorldServerNetwork(PacketProcessor packetProcessor)
        {
            var peer = _serverService.Startup(ServerConstants.MasterWorldPort);
            if (peer == IntPtr.Zero)
                return null;

            var networkManager = new NetworkManager(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _serviceProvider.GetRequiredService<ILogService>(),
                _serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Make sure clients can't send packets meant for master<->world communication.
            networkManager.ClaimPacketID(PacketIdentifier.ID_REGISTER_WORLD);

            // Initialize the packet sender for communication with world servers.
            var packetSender = _serviceProvider.GetRequiredService<WorldPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, _serverService.Shutdown);
            return networkManager;
        }

        private NetworkManager? CreateClientNetwork(PacketProcessor packetProcessor)
        {
            var peer = _serverService.Startup(ServerConstants.MasterClientPort);
            if (peer == IntPtr.Zero)
                return null;

            var networkManager = new NetworkManager(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _serviceProvider.GetRequiredService<ILogService>(),
                _serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Initialize the packet sender for communication with clients.
            var packetSender = _serviceProvider.GetRequiredService<ClientPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, _serverService.Shutdown);
            return networkManager;
        }
    }
}
