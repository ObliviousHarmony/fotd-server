using System.Xml.Linq;
using FOMServer.Shared.Core.Items;
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
            player.Attributes.AttributesChanged += OnAttributesChanged;
            player.Inventory.ItemDestroyed += OnInventoryItemDestroyed;
        }

        public void Unregister(Player player)
        {
            player.Attributes.AttributesChanged -= OnAttributesChanged;
            player.Inventory.ItemDestroyed += OnInventoryItemDestroyed;
        }

        private void OnAttributesChanged(PlayerAttributes attributes)
        {
            _logger.LogInformation("Player {PlayerId}'s attributes updated", attributes.PlayerId);
        }

        private void OnInventoryItemDestroyed(PlayerInventory inventory, Item item)
        {
            _logger.LogInformation(
                "Item {ItemId} in player {PlayerId}'s inventory was destroyed",
                item.Id,
                inventory.PlayerId
            );
        }
    }
}
