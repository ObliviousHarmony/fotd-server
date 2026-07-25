using System.Text;
using System.Xml.Linq;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    internal class PlayerEventPacketDispatcher : IPlayerEventPacketDispatcher
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<PlayerEventPacketDispatcher> _logger;

        public PlayerEventPacketDispatcher(
            IClientPacketSender clientPacketSender,
            ILogger<PlayerEventPacketDispatcher> logger
        )
        {
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public void Register(Player player)
        {
            player.Avatar.AvatarChanged += OnPlayerAvatarChange;
            player.Attributes.AttributesChanged += OnAttributesChanged;
        }

        public void Unregister(Player player)
        {
            player.Avatar.AvatarChanged -= OnPlayerAvatarChange;
            player.Attributes.AttributesChanged -= OnAttributesChanged;
        }

        private void OnAttributesChanged(PlayerAttributes attributes, long changedAttributeMask)
        {
            var sb = new StringBuilder();
            for (var i = AttributeType.Health; i < AttributeType.NUM_ATTRIBUTE_TYPES; ++i)
            {
                if (!i.IsMaskSet(changedAttributeMask))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(i);
            }

            _logger.LogInformation("Player {PlayerId}'s attributes updated ({Attributes})", attributes.PlayerId, sb);
        }

        private void OnPlayerAvatarChange(PlayerAvatar avatar)
        {
            _logger.LogInformation("Player {PlayerId}'s avatar changed", avatar.PlayerId);
        }
    }
}
