using FOMServer.Master.Application.Handlers;
using FOMServer.Master.Application.Players;
using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Core.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace FOMServer.Master.Tests
{
    public class ConnectionLifecycleTests
    {
        private const uint PlayerID = 42;

        [Fact]
        public void Connect_Login_Disconnect_DrivesSessionAndPlayerLifecycle()
        {
            var fixture = new Fixture();
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            // 1. Connect: NewIncomingConnection registers a bare session.
            fixture.NewConnection.Handle(address, new NewIncomingConnection());

            var session = fixture.ClientRegistry.Get(address);
            Assert.NotNull(session);
            Assert.False(session!.IsLoggingIn);
            Assert.False(session.IsReady);
            Assert.Null(fixture.PlayerRegistry.Get(PlayerID));

            // 2. Login: starts login, loads + completes the player, replies.
            fixture.Login.Handle(address, new Login());

            Assert.True(session.IsReady);
            Assert.NotNull(session.Player);
            Assert.Equal(PlayerID, session.Player!.ID);
            Assert.Same(session.Player, fixture.PlayerRegistry.Get(PlayerID));
            Assert.Contains(PacketHelpers.GetPacketTypeID<LoginReturn>(), fixture.Sender.Sent);

            // 3. Disconnect: unregisters the session and logs the player out.
            fixture.Disconnect.Handle(address, new DisconnectionNotification());

            Assert.Null(fixture.ClientRegistry.Get(address));
            Assert.Null(fixture.PlayerRegistry.Get(PlayerID));
        }

        [Fact]
        public void ConnectionLost_TearsDownSessionAndPlayer()
        {
            var fixture = new Fixture();
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            fixture.NewConnection.Handle(address, new NewIncomingConnection());
            fixture.Login.Handle(address, new Login());
            Assert.NotNull(fixture.PlayerRegistry.Get(PlayerID));

            fixture.ConnectionLost.Handle(address, new ConnectionLost());

            Assert.Null(fixture.ClientRegistry.Get(address));
            Assert.Null(fixture.PlayerRegistry.Get(PlayerID));
        }

        [Fact]
        public void Login_WithoutSession_IsDroppedWithoutThrowing()
        {
            var fixture = new Fixture();
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            // No NewIncomingConnection first, so there's no session.
            fixture.Login.Handle(address, new Login());

            Assert.Null(fixture.ClientRegistry.Get(address));
            Assert.Empty(fixture.Sender.Sent);
        }

        private sealed class Fixture
        {
            public Fixture()
            {
                Sender = new RecordingPacketSender();
                ClientRegistry = new ClientRegistry();

                var persistence = new Mock<IPersistenceService>();

                // Flush synchronously so logout removal happens within the disconnect call.
                _ = persistence
                    .Setup(s => s.WaitForPersistence(It.IsAny<IPersistable>(), It.IsAny<Action>()))
                    .Callback<IPersistable, Action>((_, callback) => callback());
                PlayerRegistry = new PlayerRegistry(persistence.Object);

                var accounts = new Mock<IAccountRepository>();
                _ = accounts.Setup(r => r.GetByUsername(It.IsAny<string>())).Returns(new AccountDto { id = PlayerID });

                var players = new Mock<IPlayerRepository>();
                _ = players.Setup(r => r.GetByID(PlayerID)).Returns(new PlayerDto { id = PlayerID, name = "Tester" });

                // IWorldServerRegistry is an internal interface; mocking it would need an
                // unsigned DynamicProxy assembly that the keyed InternalsVisibleTo doesn't cover.
                // A hand-written stub sidesteps that and is all the disconnect path needs.
                var worlds = new EmptyWorldRegistry();

                NewConnection = new NewIncomingConnectionHandler(
                    ClientRegistry, NullLogger<NewIncomingConnectionHandler>.Instance);
                Login = new LoginHandler(
                    accounts.Object, players.Object, ClientRegistry, PlayerRegistry, Sender,
                    NullLogger<LoginHandler>.Instance);
                Disconnect = new DisconnectionHandler(
                    worlds, ClientRegistry, PlayerRegistry,
                    NullLogger<DisconnectionHandler>.Instance);
                ConnectionLost = new ConnectionLostHandler(
                    worlds, ClientRegistry, PlayerRegistry,
                    NullLogger<ConnectionLostHandler>.Instance);
            }

            public RecordingPacketSender Sender { get; }

            public ClientRegistry ClientRegistry { get; }

            public PlayerRegistry PlayerRegistry { get; }

            public NewIncomingConnectionHandler NewConnection { get; }

            public LoginHandler Login { get; }

            public DisconnectionHandler Disconnect { get; }

            public ConnectionLostHandler ConnectionLost { get; }
        }

        private sealed class EmptyWorldRegistry : IWorldServerRegistry
        {
            public WorldServer[] GetAll()
            {
                return [];
            }

            public WorldServer? Get(WorldID id)
            {
                return null;
            }

            public WorldID[] Register(WorldID[] ids, NetworkAddress serverAddress, NetworkAddress publicAddress)
            {
                return [];
            }

            public WorldID[] Unregister(NetworkAddress serverAddress)
            {
                return [];
            }
        }

        private sealed class RecordingPacketSender : IClientPacketSender
        {
            public List<PacketIdentifier> Sent { get; } = [];

            public void Send(in QueuePacket packet)
            {
                Sent.Add(packet.ID);
                packet.Release();
            }

            public void Broadcast(in QueuePacket packet)
            {
                Sent.Add(packet.ID);
                packet.Release();
            }
        }
    }
}
