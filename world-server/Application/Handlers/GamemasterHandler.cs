using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Items;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class GamemasterHandler : PacketHandlerBase<Gamemaster>
    {
        // ID_ITEMS_ADDED `dest` selector the client's HandlePacket_ID_ITEMS_ADDED
        // switches on to decide where added items go and how to refresh. Value 1 is
        // the "merge into PLAYERDATA_INVENTORY and refresh" path (the backpack).
        // Note: there is no case 0 — sending 0 makes the client silently drop the
        // items. See "Game Master Commands" / "Inventory" in the client vault.
        private const byte AddToInventory = 1;

        private readonly IPlayerRegistry _playerRegistry;
        private readonly IItemInstanceIdGenerator _itemInstanceIdGenerator;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<GamemasterHandler> _logger;

        public GamemasterHandler(
            IPlayerRegistry playerRegistry,
            IItemInstanceIdGenerator itemInstanceIdGenerator,
            IClientPacketSender clientPacketSender,
            ILogger<GamemasterHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _itemInstanceIdGenerator = itemInstanceIdGenerator;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in Gamemaster p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received GM command from unregistered client '{Sender}'", sender);
                return;
            }

            if (p.PlayerId != player.Id)
            {
                _logger.LogWarning("Player {PlayerId} sent a GM command for {OtherId}", player.Id, p.PlayerId);
                return;
            }

            // All GM commands share this packet id; only /spawn is understood so
            // far. The client already gates access (see the client vault); there
            // is no authoritative server-side access check yet.
            if (p.Command != GamemasterCommand.Spawn)
            {
                _logger.LogDebug("Ignoring unhandled GM command {Command} from player {PlayerId}", p.Command, player.Id);
                return;
            }

            HandleSpawn(sender, player, p);
        }

        private void HandleSpawn(NetworkAddress sender, Player player, in Gamemaster p)
        {
            var quantity = (int)Math.Min(p.Quantity, (uint)BufferSizes.MaxItemListSize);
            if (quantity <= 0)
            {
                _logger.LogDebug("Player {PlayerId} spawned item type {Type} with zero quantity", player.Id, p.Item.Base.Type);
                return;
            }

            // Trust the item template the client built from its catalog (the server
            // has no item-definition data yet); only the instance id is server-owned.
            var itemBase = p.Item.Base;

            // Each spawned instance is one physical unit. The client's pickup
            // notification amount is `value * instanceCount` (ItemBase.value at
            // ItemStack+0x2 times the ids-set size, per HandlePacket_ID_ITEMS_ADDED),
            // and the client sends value 0 for non-stackable items — which shows a
            // "+0" popup. Floor it at 1 so the count reflects reality while still
            // honouring a real stack value when the client provides one.
            if (itemBase.Value < 1)
            {
                itemBase.Value = 1;
            }

            using var response = new PacketWriter<ItemsAdded>(sender);
            ref var rData = ref response.Data;
            rData.PlayerId = player.Id;
            rData.Dest = AddToInventory;
            rData.Items.ItemCount = (uint)quantity;

            for (var i = 0; i < quantity; i++)
            {
                var item = new Item
                {
                    Id = _itemInstanceIdGenerator.Next(),
                    Base = itemBase,
                };
                player.AddItem(item);
                rData.Items.Items[i] = item;
            }

            _logger.LogInformation(
                "Player {PlayerId} spawned {Quantity}x item type {Type} (value={Value})",
                player.Id, quantity, itemBase.Type, p.Item.Base.Value);
            _clientPacketSender.Send(response.Build());
        }
    }
}
