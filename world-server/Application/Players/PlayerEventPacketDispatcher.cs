using System.Collections.Concurrent;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    internal class PlayerEventPacketDispatcher : IPlayerEventPacketDispatcher
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<PlayerEventPacketDispatcher> _logger;

        private readonly ConcurrentDictionary<uint, NetworkAddress> _addresses;

        public PlayerEventPacketDispatcher(
            IClientPacketSender clientPacketSender,
            ILogger<PlayerEventPacketDispatcher> logger
        )
        {
            _clientPacketSender = clientPacketSender;
            _logger = logger;

            _addresses = new();
        }

        public void Register(Player player)
        {
            if (player.Address == NetworkAddress.Unassigned)
            {
                throw new InvalidOperationException(
                    $"Player {player.Id} must be claimed by a client before its events can be dispatched"
                );
            }

            _addresses[player.Id] = player.Address;

            player.Avatar.AvatarChanged += OnPlayerAvatarChange;
            player.Attributes.AttributesChanged += OnAttributesChanged;
        }

        public void Unregister(Player player)
        {
            player.Avatar.AvatarChanged -= OnPlayerAvatarChange;
            player.Attributes.AttributesChanged -= OnAttributesChanged;

            _addresses.TryRemove(player.Id, out _);
        }

        private void OnAttributesChanged(PlayerAttributes attributes, long changedAttributeMask)
        {
            if (changedAttributeMask == 0)
            {
                return;
            }

            if (!TryGetAddress(attributes.PlayerId, out var address))
            {
                return;
            }

            using var packet = new PacketWriter<AttributeChangePacket>(address);
            ref var rData = ref packet.Data;

            for (var i = AttributeType.Health; i < AttributeType.NUM_ATTRIBUTE_TYPES; ++i)
            {
                if (!i.IsMaskSet(changedAttributeMask))
                {
                    continue;
                }

                rData.Set(i, attributes.Get(i));
            }

            _clientPacketSender.Send(packet.Build());
        }

        private void OnPlayerAvatarChange(PlayerAvatar avatar)
        {
            if (!TryGetAddress(avatar.PlayerId, out var address))
            {
                return;
            }

            using var packet = new PacketWriter<AvatarChangePacket>(address);
            ref var rData = ref packet.Data;

            rData.PlayerId = avatar.PlayerId;
            avatar.WriteTo(ref rData.Avatar);

            _clientPacketSender.Send(packet.Build());
        }

        private bool TryGetAddress(uint playerId, out NetworkAddress address)
        {
            if (_addresses.TryGetValue(playerId, out address))
            {
                return true;
            }

            _logger.LogWarning("Dropped packet for unregistered player {PlayerId}", playerId);
            return false;
        }
    }
}
