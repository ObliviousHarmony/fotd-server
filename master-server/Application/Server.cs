using FluentMigrator.Runner;
using FOMServer.Master.Application.Networking;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Infrastructure.FOMNetwork;

namespace FOMServer.Master.Application
{
    internal class Server
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

            _logger.LogInformation("Starting master server");

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _shutdownManager.StartShutdown();
            };

            if (!InitializeDatabase())
            {
                await _shutdownManager.Shutdown();
                return;
            }

            var packetProcessor = new PacketProcessor(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _serviceProvider.GetRequiredService<ILogger<PacketProcessor>>(),
                _serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            var worldNetwork = CreateWorldServerNetwork(packetProcessor);
            if (worldNetwork is null)
            {
                _logger.LogCritical("Failed to create the world server network");
                await _shutdownManager.Shutdown();
                return;
            }

            var clientNetwork = CreateClientNetwork(packetProcessor);
            if (clientNetwork is null)
            {
                _logger.LogCritical("Failed to create the client network");
                await _shutdownManager.Shutdown();
                return;
            }

            // The server is now ready to start processing packets.
            packetProcessor.Start();
            worldNetwork.Start();
            clientNetwork.Start();

            foreach (var startable in _serviceProvider.GetServices<IServerStartable>())
            {
                startable.Start();
            }

            _logger.LogInformation("Server started");
            _logger.LogInformation("Press Ctrl + C to stop the server");

            await _shutdownManager.Stopped;
        }

        private bool InitializeDatabase()
        {
            try
            {
                if (!_migrationRunner.HasMigrationsToApplyUp())
                {
                    return true;
                }

                _migrationRunner.MigrateUp();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Database migration failed");
                return false;
            }

            return true;
        }

        private NetworkManager? CreateWorldServerNetwork(PacketProcessor packetProcessor)
        {
            // Master<->world packets are less sensitive to latency and we can sleep the network thread
            // longer in order to avoid burning cycles unnecessarily.
            var peer = _serverService.Startup(ServerConstants.MasterWorldPort, (uint)WorldId.NUM_WORLDS - 1, 50);
            if (peer == IntPtr.Zero)
            {
                return null;
            }

            var networkManager = new NetworkManager(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _serviceProvider.GetRequiredService<ILogger<NetworkManager>>(),
                _serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Make sure clients can't send packets meant for master<->world communication.
            networkManager.ClaimPacketId(PacketIdentifier.ID_REGISTER_WORLD);
            networkManager.ClaimPacketId(PacketIdentifier.ID_PLAYER_WORLD_READY);
            networkManager.ClaimPacketId(PacketIdentifier.ID_PLAYER_MIGRATE_WORLD);

            // Initialize the packet sender for communication with world servers.
            var packetSender = _serviceProvider.GetRequiredService<WorldPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, _serverService.Shutdown);

            _logger.LogInformation("Listening for world servers");

            return networkManager;
        }

        private NetworkManager? CreateClientNetwork(PacketProcessor packetProcessor)
        {
            // Master<->client packets are only user interface related and aren't latency sensitive.
            var peer = _serverService.Startup(ServerConstants.MasterClientPort, 100, 50);
            if (peer == IntPtr.Zero)
            {
                return null;
            }

            var networkManager = new NetworkManager(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _serviceProvider.GetRequiredService<ILogger<NetworkManager>>(),
                _serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Clients connecting produce NewIncomingConnection. Claim it on the client network
            // so world-server connections (on the other manager) don't get registered as client sessions.
            networkManager.ClaimPacketId(
                PacketIdentifier.ID_NEW_INCOMING_CONNECTION,
                NetworkManager.PacketClaimBehavior.IgnoreSilently
            );

            // Initialize the packet sender for communication with clients.
            var packetSender = _serviceProvider.GetRequiredService<ClientPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, _serverService.Shutdown);

            _logger.LogInformation("Listening for clients");

            return networkManager;
        }
    }
}
