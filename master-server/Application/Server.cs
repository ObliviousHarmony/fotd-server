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
        private readonly ClientPacketSender clientPacketSender;
        private readonly WorldPacketSender worldPacketSender;
        private readonly IServiceProvider serviceProvider;

        public Server(
            ILogService logService,
            IMigrationRunner migrationRunner,
            ServerSettings serverSettings,
            INetworkService networkService,
            IServerService serverService,
            ClientPacketSender clientPacketSender,
            WorldPacketSender worldPacketSender,
            IServiceProvider serviceProvider
        )
        {
            this.logService = logService;
            this.migrationRunner = migrationRunner;
            this.serverSettings = serverSettings;
            this.networkService = networkService;
            this.serverService = serverService;
            this.clientPacketSender = clientPacketSender;
            this.worldPacketSender = worldPacketSender;
            this.serviceProvider = serviceProvider;
        }

        public void Run()
        {
            var cts = new CancellationTokenSource();

            logService.WriteMessage(LogLevel.Info, "Starting Master Server...");
            logService.WriteMessage(LogLevel.Info, "Press Ctrl+C for shutdown.");

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

            // We need to make sure our packet structs are all blittable and match the C++ side.
            // This is critical to ensure that we don't have memory corruption and don't
            // require expensive marshalling of data between managed and unmanaged code.
            networkService.ValidateFOMPacket();

            var packetProcessor = new PacketProcessor(
                logService,
                serviceProvider.GetRequiredService<IEnumerable<IPacketHandler>>()
            );

            if (!StartWorldServerNetwork(cts.Token, packetProcessor))
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to start the world server network.");
                return;
            }

            if (!StartClientNetwork(cts.Token, packetProcessor))
            {
                logService.WriteMessage(LogLevel.Critical, "Failed to start the client network.");
                return;
            }

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

        private bool StartWorldServerNetwork(CancellationToken ctParent, PacketProcessor packetProcessor)
        {
            var peer = serverService.Startup(serverSettings.WorldPort);
            if (peer == IntPtr.Zero)
                return false;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            worldPacketSender.Initialize(networkManager);
            packetProcessor.Start(ctParent);
            networkManager.Start(ctParent, peer, serverService.Shutdown);

            logService.WriteMessage(LogLevel.Info, $"World Port: {serverSettings.WorldPort}");

            return true;
        }

        private bool StartClientNetwork(CancellationToken ctParent, PacketProcessor packetProcessor)
        {
            var peer = serverService.Startup(serverSettings.ClientPort);
            if (peer == IntPtr.Zero)
                return false;

            var networkManager = new NetworkManager(
                serviceProvider.GetRequiredService<IPacketService>(),
                packetProcessor
            );

            clientPacketSender.Initialize(networkManager);
            packetProcessor.Start(ctParent);
            networkManager.Start(ctParent, peer, serverService.Shutdown);

            logService.WriteMessage(LogLevel.Info, $"Client Port: {serverSettings.ClientPort}");

            return true;
        }
    }
}
