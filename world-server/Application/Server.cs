using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Networking;
using FOMServer.World.Application.Networking;
using FOMServer.World.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.World.Application
{
    public class Server
    {
        private readonly ILogService _logService;
        private readonly IShutdownManager _shutdownManager;
        private readonly ServerSettings _serverSettings;
        private readonly INetworkService _networkService;
        private readonly IServerService _serverService;
        private readonly IClientService _clientService;
        private readonly IServiceProvider _serviceProvider;

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
            _logService = logService;
            _shutdownManager = shutdownManager;
            _serverSettings = serverSettings;
            _networkService = networkService;
            _serverService = serverService;
            _clientService = clientService;
            _serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            _networkService.ValidateFOMPacket();

            _logService.WriteMessage(LogLevel.Info, "------------------------------------------------");
            _logService.WriteMessage(LogLevel.Info, "Initializing World Server");

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

            var packetProcessor = new PacketProcessor(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _logService,
                _serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            var masterNetwork = ConnectToMasterNetwork(packetProcessor);
            if (masterNetwork == null)
            {
                _logService.WriteMessage(LogLevel.Critical, "Failed to connect to the master server.");
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
            masterNetwork.Start();
            clientNetwork.Start();

            _logService.WriteMessage(LogLevel.Info, $"Master Server: {_serverSettings.MasterServerAddress}:{_serverSettings.MasterServerPort}");
            _logService.WriteMessage(LogLevel.Info, $"Client Port: {_serverSettings.ClientPort}");
            _logService.WriteMessage(LogLevel.Info, "------------------------------------------------");

            await _shutdownManager.Stopped;
            _logService.WriteMessage(LogLevel.Info, "Shutdown Complete");
        }

        private NetworkManager? ConnectToMasterNetwork(PacketProcessor packetProcessor)
        {
            IntPtr peer = IntPtr.Zero;
            while (peer == IntPtr.Zero)
            {
                peer = _clientService.Connect(_serverSettings.MasterServerAddress, _serverSettings.MasterServerPort);
                if (peer == IntPtr.Zero)
                {
                    _logService.WriteMessage(LogLevel.Critical, "Failed to connect to master server, retrying in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }

            var networkManager = new NetworkManager(
                _serviceProvider.GetRequiredService<IShutdownManager>(),
                _serviceProvider.GetRequiredService<ILogService>(),
                _serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            // Make sure clients can't send packets meant for master<->world communication.
            networkManager.ClaimPacketID(PacketIdentifier.ID_PLAYER_ENTERING_WORLD);

            // Initialize the packet sender for communication with the master server.
            var packetSender = _serviceProvider.GetRequiredService<MasterPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, _clientService.Disconnect);

            // Register this world server with the master server.
            packetSender.Send(
                new RegisterWorld
                {
                    WorldID = _serverSettings.WorldID,
                    Port = _serverSettings.ClientPort,
                    Address = _serverSettings.ClientAddress
                },
                PacketPriority.MEDIUM_PRIORITY,
                PacketReliability.RELIABLE
            );

            return networkManager;
        }

        private NetworkManager? CreateClientNetwork(PacketProcessor packetProcessor)
        {
            var peer = _serverService.Startup(_serverSettings.ClientPort);
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
