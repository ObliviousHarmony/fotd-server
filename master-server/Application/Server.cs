using FluentMigrator.Runner;
using FOMServer.Master.Application.Networking;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using MySqlConnector;

namespace FOMServer.Master.Application
{
    public class Server
    {
        private readonly ILogger<Server> _logger;
        private readonly IShutdownManager _shutdownManager;
        private readonly IMigrationRunner _migrationRunner;
        private readonly INetworkService _networkService;
        private readonly IServerService _serverService;
        private readonly IServiceProvider _serviceProvider;

        public Server(
            ILogger<Server> logger,
            IShutdownManager shutdownManager,
            IMigrationRunner migrationRunner,
            INetworkService networkService,
            IServerService serverService,
            IServiceProvider serviceProvider
        )
        {
            _logger = logger;
            _shutdownManager = shutdownManager;
            _migrationRunner = migrationRunner;
            _networkService = networkService;
            _serverService = serverService;
            _serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            Console.Title = "Master Server";

            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            _networkService.ValidatePacketStructs();

            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Initializing Master Server");

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Stopping Server...");

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
                _serviceProvider.GetRequiredService<ILogger<PacketProcessor>>(),
                _serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            var worldNetwork = CreateWorldServerNetwork(packetProcessor);
            if (worldNetwork == null)
            {
                _logger.LogCritical("Failed to create the world server network");
                return;
            }

            var clientNetwork = CreateClientNetwork(packetProcessor);
            if (clientNetwork == null)
            {
                _logger.LogCritical("Failed to create the client network");
                return;
            }

            // The server is now ready to start processing packets.
            packetProcessor.Start();
            worldNetwork.Start();
            clientNetwork.Start();

            foreach (var startable in _serviceProvider.GetServices<IServerStartable>())
                startable.Start();

            Console.WriteLine("------------------------------------------------");

            await _shutdownManager.Stopped;
            Console.WriteLine("Shutdown Complete");
        }

        private bool InitializeDatabase()
        {
            try
            {
                _migrationRunner.MigrateUp();
            }
            catch (MySqlException)
            {
                _logger.LogCritical("Failed to connect to the database. Please check your connection settings");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to apply database migrations");
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
                _serviceProvider.GetRequiredService<ILogger<NetworkManager>>(),
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
                _serviceProvider.GetRequiredService<ILogger<NetworkManager>>(),
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
