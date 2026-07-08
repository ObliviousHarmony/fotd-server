using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Networking;
using FOMServer.World.Application.Players;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;
using PacketWorldUpdate = FOMServer.Shared.Core.Packets.Types.WorldUpdate;
using WorldUpdatePacket = FOMServer.Shared.Core.Packets.WorldUpdate;

namespace FOMServer.World.Tests
{
    public class PlayerUpdateServiceTests
    {
        [Fact]
        public async Task ChangedPlayer_ReachesOthers_NotItself()
        {
            var fixture = new Fixture();
            var a = fixture.AddPlayer(1, 5001);
            fixture.AddPlayer(2, 5002);

            Move(a, animation: 7);
            fixture.Service.QueueUpdate(a);

            await fixture.Service.TickAsync(CancellationToken.None);

            // Only B receives; the mover is not sent its own update.
            var capture = Assert.Single(fixture.Sender.Sends);
            Assert.Equal(2u, capture.PlayerId);
            Assert.Equal(1, capture.UpdateCount);
            Assert.Equal(1u, capture.Entries[0].Character.Id);
            Assert.Equal(7, capture.Entries[0].Character.AnimationId);
        }

        [Fact]
        public async Task RepeatedRecord_SendsOnlyLatestState()
        {
            var fixture = new Fixture();
            var a = fixture.AddPlayer(1, 5001);
            fixture.AddPlayer(2, 5002);

            Move(a, animation: 10);
            fixture.Service.QueueUpdate(a);
            Move(a, animation: 20);
            fixture.Service.QueueUpdate(a);

            await fixture.Service.TickAsync(CancellationToken.None);

            var capture = Assert.Single(fixture.Sender.Sends);
            Assert.Equal(1, capture.UpdateCount);
            Assert.Equal(20, capture.Entries[0].Character.AnimationId);
        }

        [Fact]
        public async Task MultipleMovers_EachRecipientGetsTheOthers()
        {
            var fixture = new Fixture();
            var a = fixture.AddPlayer(1, 5001);
            fixture.AddPlayer(2, 5002);
            var c = fixture.AddPlayer(3, 5003);

            Move(a, animation: 1);
            fixture.Service.QueueUpdate(a);
            Move(c, animation: 3);
            fixture.Service.QueueUpdate(c);

            await fixture.Service.TickAsync(CancellationToken.None);

            Assert.Equal(3, fixture.Sender.Sends.Count);
            var byRecipient = fixture.Sender.Sends.ToDictionary(s => s.PlayerId);
            Assert.Equal(new uint[] { 3 }, byRecipient[1].Entries.Select(e => e.Character.Id).ToArray());
            Assert.Equal(new uint[] { 1, 3 }, byRecipient[2].Entries.Select(e => e.Character.Id).ToArray());
            Assert.Equal(new uint[] { 1 }, byRecipient[3].Entries.Select(e => e.Character.Id).ToArray());
        }

        [Fact]
        public async Task LoggedOutPlayer_StopsReceiving()
        {
            var fixture = new Fixture();
            var a = fixture.AddPlayer(1, 5001);
            fixture.AddPlayer(2, 5002);
            var c = fixture.AddPlayer(3, 5003);

            // First tick: C is a registered recipient and receives A's update.
            Move(a, animation: 1);
            fixture.Service.QueueUpdate(a);
            await fixture.Service.TickAsync(CancellationToken.None);
            Assert.Contains(fixture.Sender.Sends, s => s.PlayerId == 3);

            fixture.Sender.Sends.Clear();
            fixture.Service.UnregisterRecipient(c);

            // Second tick: after unregistering, the departed C receives nothing while the
            // still-connected B keeps receiving.
            Move(a, animation: 2);
            fixture.Service.QueueUpdate(a);
            await fixture.Service.TickAsync(CancellationToken.None);

            Assert.Contains(fixture.Sender.Sends, s => s.PlayerId == 2);
            Assert.DoesNotContain(fixture.Sender.Sends, s => s.PlayerId == 3);
        }

        [Fact]
        public async Task ReRecordAfterTick_SendsAgain()
        {
            var fixture = new Fixture();
            var a = fixture.AddPlayer(1, 5001);
            fixture.AddPlayer(2, 5002);

            Move(a, animation: 10);
            fixture.Service.QueueUpdate(a);
            await fixture.Service.TickAsync(CancellationToken.None);
            Assert.Single(fixture.Sender.Sends);

            Move(a, animation: 20);
            fixture.Service.QueueUpdate(a);
            await fixture.Service.TickAsync(CancellationToken.None);

            Assert.Equal(2, fixture.Sender.Sends.Count);
            Assert.Equal(20, fixture.Sender.Sends[1].Entries[0].Character.AnimationId);
        }

        [Fact]
        public async Task RecipientOverPacketCap_GetsEveryUpdateAcrossMultiplePackets()
        {
            var fixture = new Fixture();

            // A recipient that does not move, plus more movers than fit in a single packet.
            var recipient = fixture.AddPlayer(1, 5001);
            const int moverCount = 150;
            for (uint i = 0; i < moverCount; i++)
            {
                var mover = fixture.AddPlayer(100 + i, (ushort)(6000 + i));
                Move(mover, animation: 1);
                fixture.Service.QueueUpdate(mover);
            }

            await fixture.Service.TickAsync(CancellationToken.None);

            var packets = fixture.Sender.Sends.Where(s => s.PlayerId == recipient.Id).ToList();

            // ceil(150 / 100) = 2 packets, each within the cap, with every mover present and none dropped.
            Assert.Equal(2, packets.Count);
            Assert.All(packets, p => Assert.True(p.UpdateCount <= WorldUpdatePacket.MaxWorldUpdates));
            Assert.Equal(moverCount, packets.Sum(p => (int)p.UpdateCount));
            var received = packets.SelectMany(p => p.Entries.Select(e => e.Character.Id)).ToHashSet();
            Assert.True(received.SetEquals(Enumerable.Range(100, moverCount).Select(i => (uint)i)));
        }

        private static void Move(Player player, ushort animation)
        {
            player.ApplyUpdate(new PacketWorldUpdate.PlayerUpdate { Character = new() { AnimationId = animation } });
        }

        private sealed class Fixture
        {
            public Fixture()
            {
                Service = new PlayerUpdateService(Sender);
            }

            public CapturingSender Sender { get; } = new();

            public PlayerUpdateService Service { get; }

            public Player AddPlayer(uint id, ushort port)
            {
                var player = new Player(id);
                player.ClaimForClient(new NetworkAddress { BinaryAddress = 0x0100007F, Port = port });
                Service.RegisterRecipient(player);
                return player;
            }
        }

        private sealed record Capture(uint PlayerId, byte UpdateCount, PacketWorldUpdate[] Entries);

        private sealed class CapturingSender : IClientPacketSender
        {
            public List<Capture> Sends { get; } = [];

            public void Send(in QueuePacket packet)
            {
                // Decode immediately: the packet's buffer is pooled and released here.
                var update = MemoryMarshal.Read<WorldUpdatePacket>(packet.Data);

                var entries = new PacketWorldUpdate[update.UpdateCount];
                for (var i = 0; i < update.UpdateCount; i++)
                {
                    entries[i] = update.Updates[i];
                }

                Sends.Add(new Capture(update.PlayerId, update.UpdateCount, entries));
                packet.Release();
            }

            public void CloseConnection(in NetworkAddress address)
            {
                throw new NotImplementedException();
            }
        }
    }
}
