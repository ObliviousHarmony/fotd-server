using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Application.Players;
using FOMServer.World.Core.Players;
using FOMServer.World.Core.Players.Registration;
using FOMServer.World.Tests.Factories;
using Microsoft.Extensions.Time.Testing;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.World.Tests.Players
{
    public class PlayerRegistryTests
    {
        private const uint PlayerId = 42;
        private const uint ClientBinary = 0x0100007F;

        [Fact]
        public void PrepareThenClaim_WithMatchingAddress_RunsTheFullCycle()
        {
            var fixture = new PlayerRegistryFixture();

            fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            // A pending player is unreachable through either lookup.
            Assert.Null(fixture.Registry.Get(PlayerId));
            Assert.Null(fixture.Registry.Get(Address()));

            var player = fixture.Registry.ClaimForClient(PlayerId, Address());

            Assert.NotNull(player);
            Assert.Same(player, fixture.Registry.Get(PlayerId));
            Assert.Same(player, fixture.Registry.Get(Address()));
        }

        [Fact]
        public void Claim_MatchingBinaryAddressDifferentPort_Activates()
        {
            var fixture = new PlayerRegistryFixture();
            fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            // The world sees the client through a different socket, so only the IP is gated.
            var sender = Address(port: 51234);
            var player = fixture.Registry.ClaimForClient(PlayerId, sender);

            Assert.NotNull(player);
            Assert.Same(player, fixture.Registry.Get(sender));
        }

        [Fact]
        public void Claim_WrongBinaryAddress_ReturnsNullAndLeavesPendingIntact()
        {
            var fixture = new PlayerRegistryFixture();
            fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            Assert.Null(fixture.Registry.ClaimForClient(PlayerId, Address(binary: 0x02000010)));
            Assert.Null(fixture.Registry.Get(PlayerId));

            // The legitimate client can still take over.
            Assert.NotNull(fixture.Registry.ClaimForClient(PlayerId, Address()));
        }

        [Fact]
        public void Claim_AfterTimeout_ReturnsNullAndDropsEntry()
        {
            var fixture = new PlayerRegistryFixture();
            fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            fixture.Time.Advance(TimeSpan.FromHours(1));

            Assert.Null(fixture.Registry.ClaimForClient(PlayerId, Address()));
        }

        [Fact]
        public void Claim_JustBeforeTimeout_StillActivates()
        {
            var fixture = new PlayerRegistryFixture();
            fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            fixture.Time.Advance(TimeSpan.FromSeconds(29));

            Assert.NotNull(fixture.Registry.ClaimForClient(PlayerId, Address()));
        }

        [Fact]
        public void Prepare_Twice_ReplacesTheTakeoverAddress()
        {
            var fixture = new PlayerRegistryFixture();
            fixture.Registry.PrepareForClient(PlayerId, 0x0100007F);
            fixture.Registry.PrepareForClient(PlayerId, 0x02000010);

            // The replacement's address gates the takeover.
            Assert.Null(fixture.Registry.ClaimForClient(PlayerId, Address(binary: 0x0100007F)));
            Assert.NotNull(fixture.Registry.ClaimForClient(PlayerId, Address(binary: 0x02000010)));
        }

        private static NetworkAddress Address(uint binary = ClientBinary, ushort port = 7777)
        {
            return new NetworkAddress { BinaryAddress = binary, Port = port };
        }

        private sealed class PlayerRegistryFixture
        {
            public PlayerRegistryFixture()
            {
                Loader
                    .Setup(l => l.Load(It.IsAny<uint>()))
                    .Returns<uint>(id => TestPlayerBuilder.Create(id).Build());

                RegistrationFactory
                    .Setup(f => f.Create(It.IsAny<Player>()))
                    .Returns(new Mock<IPlayerRegistration>().Object);

                Persistence
                    .Setup(p => p.WaitForPersistence(It.IsAny<IPersistable>(), It.IsAny<Action>()))
                    .Callback<IPersistable, Action>((_, cb) => cb());

                Registry = new PlayerRegistry(Loader.Object, RegistrationFactory.Object, Time, Persistence.Object);
            }

            public Mock<IPlayerLoader> Loader { get; } = new();

            public Mock<IPlayerRegistrationFactory> RegistrationFactory { get; } = new();

            public Mock<IPersistenceService> Persistence { get; } = new();

            public FakeTimeProvider Time { get; } = new(DateTimeOffset.UnixEpoch);

            public PlayerRegistry Registry { get; }
        }
    }
}
