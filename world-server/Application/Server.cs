using System.Net;
using System.Net.Sockets;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.World.Application.Networking;
using FOMServer.World.Core;

namespace FOMServer.World.Application
{
    internal class Server
    {
        private readonly ILogger<Server> _logger;
        private readonly IShutdownManager _shutdownManager;
        private readonly ServerSettings _serverSettings;
        private readonly ushort _clientPort;
        private readonly INetworkService _networkService;
        private readonly IServerService _serverService;
        private readonly IClientService _clientService;
        private readonly IServiceProvider _serviceProvider;

        public Server(
            ILogger<Server> logger,
            IShutdownManager shutdownManager,
            ServerSettings serverSettings,
            INetworkService networkService,
            IServerService serverService,
            IClientService clientService,
            IServiceProvider serviceProvider
        )
        {
            _logger = logger;
            _shutdownManager = shutdownManager;
            _serverSettings = serverSettings;
            _networkService = networkService;
            _serverService = serverService;
            _clientService = clientService;
            _serviceProvider = serviceProvider;

            _clientPort = ServerConstants.GetWorldClientPort(_serverSettings.WorldIDs[0]);
        }

        public async Task Run()
        {
            Console.Title = $"World Server - {string.Join(", ", _serverSettings.WorldIDs)}";

            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            _networkService.ValidatePacketStructs();

            _logger.LogInformation("Starting world server");
            foreach (var worldID in _serverSettings.WorldIDs)
                _logger.LogInformation("World - {WorldID}", worldID);

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _shutdownManager.StartShutdown();
            };

            var packetProcessor = new PacketProcessor(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _serviceProvider.GetRequiredService<ILogger<PacketProcessor>>(),
                _serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            var masterNetwork = ConnectToMasterNetwork(packetProcessor);
            if (masterNetwork is null)
            {
                await _shutdownManager.Shutdown();
                return;
            }

            var clientNetwork = CreateClientNetwork(packetProcessor);
            if (clientNetwork is null)
            {
                await _shutdownManager.Shutdown();
                return;
            }

            // The server is now ready to start processing packets.
            packetProcessor.Start();
            masterNetwork.Start();
            clientNetwork.Start();

            foreach (var startable in _serviceProvider.GetServices<IServerStartable>())
                startable.Start();

            _logger.LogInformation("Server started");
            _logger.LogInformation("Press Ctrl + C to stop the server");

            await _shutdownManager.Stopped;
        }

        private NetworkManager? ConnectToMasterNetwork(PacketProcessor packetProcessor)
        {
            IntPtr peer = IntPtr.Zero;
            while (peer == IntPtr.Zero)
            {
                peer = _clientService.Connect(_serverSettings.MasterServerHost, ServerConstants.MasterWorldPort);
                if (peer == IntPtr.Zero)
                {
                    _logger.LogWarning("Failed to connect to the master server, retrying in 5 seconds");
                    Thread.Sleep(5000);
                }
            }

            var networkManager = new NetworkManager(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _serviceProvider.GetRequiredService<ILogger<NetworkManager>>(),
                _serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Initialize the packet sender for communication with the master server.
            var packetSender = _serviceProvider.GetRequiredService<MasterPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, _clientService.Disconnect);

            // Register this world server with the master server.
            using var registerPacket = new PacketWriter<RegisterWorld>();
            ref var rpData = ref registerPacket.Data;

            rpData.WorldIDCount = (byte)_serverSettings.WorldIDs.Length;
            for (int i = 0; i < _serverSettings.WorldIDs.Length; i++)
                rpData.WorldIDs[i] = _serverSettings.WorldIDs[i];
            rpData.PublicAddress = new NetworkAddress
            {
                Address = _serverSettings.ClientIP!,
                Port = _clientPort
            };

            packetSender.Send(registerPacket.Build());

            _logger.LogInformation("Registered with the master server at {MasterServerHost}", _serverSettings.MasterServerHost);

            return networkManager;
        }

        private NetworkManager? CreateClientNetwork(PacketProcessor packetProcessor)
        {
            var peer = _serverService.Startup(_clientPort);
            if (peer == IntPtr.Zero)
            {
                _logger.LogCritical("Failed to create the client network");
                return null;
            }

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

            _logger.LogInformation("Listening for clients on port {ClientPort}", _clientPort);

            return networkManager;
        }
    }
}
