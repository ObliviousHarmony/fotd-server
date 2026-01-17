using System.Net;
using System.Net.Sockets;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Infrastructure.FOMNetwork;
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
            Console.Title = $"World Server - {_serverSettings.WorldID}";

            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            _networkService.ValidatePacketStructs();

            _logService.WriteMessage(LogLevel.Info, "------------------------------------------------");
            _logService.WriteMessage(LogLevel.Info, $"Initializing World Server - {_serverSettings.WorldID} - {_serverSettings.PublicHost}");

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

            foreach (var startable in _serviceProvider.GetServices<IServerStartable>())
                startable.Start();

            _logService.WriteMessage(LogLevel.Info, "------------------------------------------------");

            await _shutdownManager.Stopped;
            _logService.WriteMessage(LogLevel.Info, "Shutdown Complete");
        }

        private NetworkManager? ConnectToMasterNetwork(PacketProcessor packetProcessor)
        {
            _logService.WriteMessage(LogLevel.Info, $"Master Server: {_serverSettings.MasterServerHost}");

            var publicHostAddresses = Dns.GetHostAddresses(_serverSettings.PublicHost, AddressFamily.InterNetwork);
            var publicIPAddress = publicHostAddresses.FirstOrDefault();
            if (publicIPAddress == null)
                throw new InvalidOperationException($"Failed to resolve public address: {_serverSettings.PublicHost}");

            IntPtr peer = IntPtr.Zero;
            while (peer == IntPtr.Zero)
            {
                peer = _clientService.Connect(_serverSettings.MasterServerHost, ServerConstants.MasterWorldPort);
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

            // Initialize the packet sender for communication with the master server.
            var packetSender = _serviceProvider.GetRequiredService<MasterPacketSender>();
            packetSender.Initialize(networkManager);

            networkManager.Configure(peer, _clientService.Disconnect);

            // Register this world server with the master server.
            using var registerPacket = new PacketWriter<RegisterWorld>();
            ref var rpData = ref registerPacket.Data;

            rpData.WorldID = _serverSettings.WorldID;
            rpData.ClientAddress = new NetworkAddress
            {
                BinaryAddress = BitConverter.ToUInt32(publicIPAddress.GetAddressBytes(), 0),
                Port = ServerConstants.GetWorldClientPort(_serverSettings.WorldID)
            };

            packetSender.Send(registerPacket.Build());

            return networkManager;
        }

        private NetworkManager? CreateClientNetwork(PacketProcessor packetProcessor)
        {
            var peer = _serverService.Startup(ServerConstants.GetWorldClientPort(_serverSettings.WorldID));
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
