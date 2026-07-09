using System.Numerics;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class MoveItemsHandler : PacketHandlerBase<MoveItems>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<MoveItemsHandler> _logger;

        public MoveItemsHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<MoveItemsHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in MoveItems p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected packet for player {PlayerId}", p.PlayerId);
                return;
            }

            if (player.Id != p.PlayerId)
            {
                _logger.LogWarning("Received invalid packet for player {PlayerId}", p.PlayerId);
                return;
            }

            var fromLocation = GetLocation(p.From, player);
            if (fromLocation is null)
            {
                _logger.LogError("Items cannot be moved from {From} / {FromSlot}", p.From, p.FromSlot);
                return;
            }

            var fromContainer = fromLocation.GetItemContainer(p.From, p.FromSlot);
            if (fromContainer is null)
            {
                _logger.LogError($"Location {p.From} does not have container {p.FromSlot} to move from");
                return;
            }

            Dictionary<uint, Item?> movedItems;
            if (p.To == ItemContainerType.Destroy)
            {
                movedItems = DestroyItems(p.Ids, fromContainer);
            }
            else
            {
                var toLocation = GetLocation(p.To, player);
                if (toLocation is null)
                {
                    _logger.LogError("Items cannot be moved to {To} / {ToSlot}", p.To, p.ToSlot);
                    return;
                }

                var toContainer = toLocation.GetItemContainer(p.To, p.ToSlot);
                if (toContainer is null)
                {
                    _logger.LogError($"Location {p.To} does not have container {p.ToSlot} to move into");
                    return;
                }

                movedItems = MoveItems(p.Ids, fromContainer, toContainer);
            }

            using var response = new PacketWriter<MoveItems>(sender);
            ref var rData = ref response.Data;

            rData.PlayerId = player.Id;
            rData.From = p.From;
            rData.FromSlot = p.FromSlot;
            rData.To = p.To;
            rData.ToSlot = p.ToSlot;

            var i = 0;
            foreach (var (id, item) in movedItems)
            {
                if (item is null)
                {
                    _logger.LogError("Failed to move item {Id} from {From} / {FromSlot} to {To} / {ToSlot}", id, p.From, p.FromSlot, p.To, p.ToSlot);
                    continue;
                }

                unsafe
                {
                    rData.RawIds[i++] = id;
                }
            }
            rData.NumIds = (ushort)i;

            _clientPacketSender.Send(response.Build());
        }

        private IItemLocation? GetLocation(ItemContainerType type, Player player)
        {
            if (type is ItemContainerType.Inventory or ItemContainerType.Weapons or ItemContainerType.Equipment)
            {
                return player.Inventory;
            }

            return null;
        }

        private Dictionary<uint, Item?> DestroyItems(ReadOnlySpan<uint> ids, ItemContainer from)
        {
            var destroyed = new Dictionary<uint, Item?>();
            foreach (var id in ids)
            {
                var item = from.Remove(id);
                destroyed.Add(id, item);
            }

            return destroyed;
        }

        private Dictionary<uint, Item?> MoveItems(ReadOnlySpan<uint> ids, ItemContainer from, ItemContainer to)
        {
            var moved = new Dictionary<uint, Item?>();
            foreach (var id in ids)
            {
                var item = from.Transfer(id, to);
                moved.Add(id, item);
            }

            return moved;
        }
    }
}
