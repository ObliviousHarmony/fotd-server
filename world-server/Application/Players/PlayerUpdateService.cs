using System.Collections.Concurrent;
using System.Threading.Channels;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Ticking;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;
using WorldUpdate = FOMServer.Shared.Core.Packets.Types.WorldUpdate;
using WorldUpdatePacket = FOMServer.Shared.Core.Packets.WorldUpdate;

namespace FOMServer.World.Application.Players
{
    internal sealed class PlayerUpdateService : IPlayerUpdateService, ITickable
    {
        private readonly IClientPacketSender _clientPacketSender;

        private readonly ConcurrentDictionary<Player, Recipient> _recipients = new();
        private readonly Channel<Recipient> _changed = Channel.CreateUnbounded<Recipient>(
            new UnboundedChannelOptions
            {
                SingleReader = true
            });

        private readonly List<Recipient> _recipientSnapshot = [];

        public PlayerUpdateService(IClientPacketSender clientPacketSender)
        {
            _clientPacketSender = clientPacketSender;
        }

        public TimeSpan TickInterval => TimeSpan.FromMilliseconds(25);

        public void RegisterRecipient(Player player)
        {
            _recipients.TryAdd(player, new Recipient(player));
        }

        public void UnregisterRecipient(Player player)
        {
            _recipients.TryRemove(player, out _);
        }

        public void QueueUpdate(Player player)
        {
            if (!_recipients.TryGetValue(player, out var recipient))
            {
                return;
            }

            // Since we only send the latest update state, there's no reason to enqueue more than once.
            if (Interlocked.Exchange(ref recipient.IsPending, 1) == 0)
            {
                _changed.Writer.TryWrite(recipient);
            }
        }

        public ValueTask TickAsync(CancellationToken cancellationToken)
        {
            // We have nothing to do during the final sweep.
            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.CompletedTask;
            }

            _recipientSnapshot.Clear();
            foreach (var (_, recipient) in _recipients)
            {
                recipient.Buffer.Clear();
                _recipientSnapshot.Add(recipient);
            }

            var hasUpdatesToSend = false;
            while (_changed.Reader.TryRead(out var source))
            {
                // A concurrent update after this point re-queues the player, so
                // its newer state is picked up next tick rather than lost.
                Interlocked.Exchange(ref source.IsPending, 0);

                var snapshot = source.Player.CaptureUpdate();
                foreach (var recipient in _recipientSnapshot)
                {
                    if (recipient.Player.Id != source.Player.Id)
                    {
                        recipient.Buffer.Add(snapshot);
                        hasUpdatesToSend = true;
                    }
                }
            }

            if (hasUpdatesToSend)
            {
                foreach (var recipient in _recipientSnapshot)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (recipient.Buffer.Count > 0)
                    {
                        SendTo(recipient.Player, recipient.Buffer);
                    }
                }
            }

            return ValueTask.CompletedTask;
        }

        private void SendTo(Player recipient, List<WorldUpdate.CharacterUpdate> sendBuffer)
        {
            for (var offset = 0; offset < sendBuffer.Count; offset += WorldUpdatePacket.MaxWorldUpdates)
            {
                var count = (byte)Math.Min(WorldUpdatePacket.MaxWorldUpdates, sendBuffer.Count - offset);

                using var writer = new PacketWriter<WorldUpdatePacket>(recipient.Address);
                ref var data = ref writer.Data;

                data.PlayerId = recipient.Id;
                data.UpdateCount = count;
                for (var i = 0; i < count; i++)
                {
                    data.Updates[i] = new WorldUpdate
                    {
                        Kind = WorldUpdate.Type.Character,
                        Character = sendBuffer[offset + i],
                    };
                }

                _clientPacketSender.Send(writer.Build());
            }
        }

        private sealed class Recipient
        {
            public int IsPending;

            public Recipient(Player player)
            {
                Player = player;
            }

            public Player Player { get; }

            public List<WorldUpdate.CharacterUpdate> Buffer { get; } = [];
        }
    }
}
