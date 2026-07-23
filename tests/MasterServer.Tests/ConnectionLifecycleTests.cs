using FOMServer.Master.Application.PacketHandlers;
using FOMServer.Master.Application.Players;
using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Core.Utilities;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Interop.FOMNetwork.Packets.RakNet;
using Microsoft.Extensions.Logging.Abstractions;

namespace FOMServer.Master.Tests
{
    public class ConnectionLifecycleTests
    {
        private const uint PlayerId = 42;

        [Fact]
        public void Connect_Login_Disconnect_DrivesSessionAndPlayerLifecycle()
        {
            var fixture = new Fixture();
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            // 1. Connect: NewIncomingConnectionPacket registers a bare session.
            fixture.NewConnection.Handle(address, new NewIncomingConnectionPacket());

            var session = fixture.ClientRegistry.Get(address);
            Assert.NotNull(session);
            Assert.False(session!.IsLoggingIn);
            Assert.False(session.IsReady);
            Assert.Null(fixture.PlayerRegistry.Get(PlayerId));

            // 2. LoginPacket: starts login, loads + completes the player, replies.
            fixture.Login.Handle(address, new LoginPacket());

            Assert.True(session.IsReady);
            Assert.NotNull(session.Player);
            Assert.Equal(PlayerId, session.Player!.Id);
            Assert.Same(session.Player, fixture.PlayerRegistry.Get(PlayerId));
            Assert.Contains(PacketHelpers.GetPacketTypeId<LoginReturnPacket>(), fixture.Sender.Sent);

            // 3. Disconnect: unregisters the session and logs the player out.
            fixture.Disconnect.Handle(address, new DisconnectionNotificationPacket());

            Assert.Null(fixture.ClientRegistry.Get(address));
            Assert.Null(fixture.PlayerRegistry.Get(PlayerId));
        }

        [Fact]
        public void ConnectionLost_TearsDownSessionAndPlayer()
        {
            var fixture = new Fixture();
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            fixture.NewConnection.Handle(address, new NewIncomingConnectionPacket());
            fixture.Login.Handle(address, new LoginPacket());
            Assert.NotNull(fixture.PlayerRegistry.Get(PlayerId));

            fixture.ConnectionLost.Handle(address, new ConnectionLostPacket());

            Assert.Null(fixture.ClientRegistry.Get(address));
            Assert.Null(fixture.PlayerRegistry.Get(PlayerId));
        }

        [Fact]
        public void Login_WithoutSession_IsDroppedWithoutThrowing()
        {
            var fixture = new Fixture();
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };

            // No NewIncomingConnectionPacket first, so there's no session.
            fixture.Login.Handle(address, new LoginPacket());

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
                persistence
                    .Setup(s => s.WaitForPersistence(It.IsAny<IPersistable>(), It.IsAny<Action>()))
                    .Callback<IPersistable, Action>((_, callback) => callback());
                PlayerRegistry = new PlayerRegistry(persistence.Object);

                var accounts = new Mock<IAccountRepository>();
                accounts.Setup(r => r.GetByUsername(It.IsAny<string>())).Returns(new AccountDto { id = PlayerId });

                var players = new Mock<IPlayerRepository>();
                players.Setup(r => r.GetById(PlayerId)).Returns(new PlayerDto { id = PlayerId, name = "Tester" });

                // IWorldServerRegistry is an internal interface; mocking it would need an
                // unsigned DynamicProxy assembly that the keyed InternalsVisibleTo doesn't cover.
                // A hand-written stub sidesteps that and is all the disconnect path needs.
                var worlds = new EmptyWorldRegistry();

                NewConnection = new NewIncomingConnectionPacketHandler(
                    ClientRegistry,
                    NullLogger<NewIncomingConnectionPacketHandler>.Instance
                );
                Login = new LoginPacketHandler(
                    accounts.Object,
                    players.Object,
                    ClientRegistry,
                    PlayerRegistry,
                    Sender,
                    NullLogger<LoginPacketHandler>.Instance
                );
                Disconnect = new DisconnectionPacketHandler(
                    worlds,
                    ClientRegistry,
                    PlayerRegistry,
                    NullLogger<DisconnectionPacketHandler>.Instance
                );
                ConnectionLost = new ConnectionLostPacketHandler(
                    worlds,
                    ClientRegistry,
                    PlayerRegistry,
                    NullLogger<ConnectionLostPacketHandler>.Instance
                );
            }

            public RecordingPacketSender Sender { get; }

            public ClientRegistry ClientRegistry { get; }

            public PlayerRegistry PlayerRegistry { get; }

            public NewIncomingConnectionPacketHandler NewConnection { get; }

            public LoginPacketHandler Login { get; }

            public DisconnectionPacketHandler Disconnect { get; }

            public ConnectionLostPacketHandler ConnectionLost { get; }
        }

        private sealed class EmptyWorldRegistry : IWorldServerRegistry
        {
            public WorldServer[] GetAll()
            {
                return [];
            }

            public WorldServer? Get(WorldId id)
            {
                return null;
            }

            public WorldId[] Register(WorldId[] ids, NetworkAddress serverAddress, NetworkAddress publicAddress)
            {
                return [];
            }

            public WorldId[] Unregister(NetworkAddress serverAddress)
            {
                return [];
            }
        }

        private sealed class RecordingPacketSender : IClientPacketSender
        {
            public List<PacketIdentifier> Sent { get; } = [];

            public void Send(in QueuePacket packet)
            {
                Sent.Add(packet.Id);
                packet.Release();
            }

            public void CloseConnection(in NetworkAddress address)
            {
                throw new NotImplementedException();
            }
        }
    }
}
