using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Application.Players;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.World.Tests
{
    public class PlayerRegistryTests
    {
        private const uint PlayerId = 42;
        private const uint ClientBinary = 0x0100007F;

        [Fact]
        public void PrepareThenClaim_WithMatchingAddress_RunsTheFullCycle()
        {
            var fixture = new Fixture();

            _ = fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

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
            var fixture = new Fixture();
            _ = fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            // The world sees the client through a different socket, so only the IP is gated.
            var sender = Address(port: 51234);
            var player = fixture.Registry.ClaimForClient(PlayerId, sender);

            Assert.NotNull(player);
            Assert.Same(player, fixture.Registry.Get(sender));
        }

        [Fact]
        public void Claim_WrongBinaryAddress_ReturnsNullAndLeavesPendingIntact()
        {
            var fixture = new Fixture();
            _ = fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            Assert.Null(fixture.Registry.ClaimForClient(PlayerId, Address(binary: 0x02000010)));
            Assert.Null(fixture.Registry.Get(PlayerId));

            // The legitimate client can still take over.
            Assert.NotNull(fixture.Registry.ClaimForClient(PlayerId, Address()));
        }

        [Fact]
        public void Claim_AfterTimeout_ReturnsNullAndDropsEntry()
        {
            var fixture = new Fixture();
            _ = fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            fixture.Time.Advance(TimeSpan.FromHours(1));

            Assert.Null(fixture.Registry.ClaimForClient(PlayerId, Address()));
        }

        [Fact]
        public void Claim_JustBeforeTimeout_StillActivates()
        {
            var fixture = new Fixture();
            _ = fixture.Registry.PrepareForClient(PlayerId, ClientBinary);

            fixture.Time.Advance(TimeSpan.FromSeconds(29));

            Assert.NotNull(fixture.Registry.ClaimForClient(PlayerId, Address()));
        }

        [Fact]
        public void Prepare_Twice_ReplacesTheTakeoverAddress()
        {
            var fixture = new Fixture();
            _ = fixture.Registry.PrepareForClient(PlayerId, 0x0100007F);
            _ = fixture.Registry.PrepareForClient(PlayerId, 0x02000010);

            // The replacement's address gates the takeover.
            Assert.Null(fixture.Registry.ClaimForClient(PlayerId, Address(binary: 0x0100007F)));
            Assert.NotNull(fixture.Registry.ClaimForClient(PlayerId, Address(binary: 0x02000010)));
        }

        private static NetworkAddress Address(uint binary = ClientBinary, ushort port = 7777)
        {
            return new NetworkAddress { BinaryAddress = binary, Port = port };
        }

        private sealed class Fixture
        {
            public Fixture()
            {
                Registry = new PlayerRegistry(new SynchronousPersistence(), Time);
            }

            public FakeTime Time { get; } = new();

            public PlayerRegistry Registry { get; }
        }

        private sealed class FakeTime : TimeProvider
        {
            private DateTimeOffset _now = DateTimeOffset.UnixEpoch;

            public override DateTimeOffset GetUtcNow()
            {
                return _now;
            }

            public void Advance(TimeSpan delta)
            {
                _now += delta;
            }
        }

        private sealed class SynchronousPersistence : IPersistenceService
        {
            public void Register(IPersistable entity)
            {
            }

            public void WaitForPersistence(IPersistable entity, Action callback)
            {
                callback();
            }
        }
    }
}
