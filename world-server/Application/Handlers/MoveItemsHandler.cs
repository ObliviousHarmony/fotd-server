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

        private readonly Dictionary<TransferKey, TransferRule> _transferRules;

        public MoveItemsHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<MoveItemsHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;

            _transferRules = BuildTransferRules();
        }

        private delegate string? TransferValidator(Player player, in MoveItems packet);

        private delegate bool TransferExecutor(Player player, in MoveItems packet);

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

            if (!_transferRules.TryGetValue(new TransferKey(p.From, p.To), out var rule))
            {
                _logger.LogWarning(
                    "Player {PlayerId}'s item(s) {ItemIds} cannot be moved from {From} / {FromSlot} to {To} / {ToSlot}, no rule found",
                    p.PlayerId,
                    string.Join(", ", p.ItemIds.ToArray()),
                    p.From,
                    p.FromSlot,
                    p.To,
                    p.ToSlot
                );

                return;
            }

            var validate = rule.Validate ?? ValidateItemContainers;
            var error = validate(player, in p);
            if (error is not null)
            {
                _logger.LogWarning(
                    "Player {PlayerId}'s item(s) {ItemIds} cannot be moved from {From} / {FromSlot} to {To} / {ToSlot}, {Reason}",
                    p.PlayerId,
                    string.Join(", ", p.ItemIds.ToArray()),
                    p.From,
                    p.FromSlot,
                    p.To,
                    p.ToSlot,
                    error
                );

                return;
            }

            var execute = rule.Execute ?? MoveBetweenContainers;
            if (!execute(player, in p))
            {
                _logger.LogWarning(
                    "Player {PlayerId}'s item(s) {ItemIds} failed to be moved from {From} / {FromSlot} to {To} / {ToSlot}",
                    p.PlayerId,
                    string.Join(", ", p.ItemIds.ToArray()),
                    p.From,
                    p.FromSlot,
                    p.To,
                    p.ToSlot
                );

                return;
            }

            // Since we did exactly what the packet asked us to, we can just
            // send it back so that the client will reflect the changes.
            using var response = new PacketWriter<MoveItems>(sender);
            response.Data = p;
            _clientPacketSender.Send(response.Build());
        }

        private Dictionary<TransferKey, TransferRule> BuildTransferRules()
        {
            var rules = new Dictionary<TransferKey, TransferRule>();

            void Rule(ItemContainerType from, ItemContainerType to, TransferValidator? validate = null, TransferExecutor? execute = null)
            {
                rules[new TransferKey(from, to)] = new TransferRule(validate, execute);
            }

            Rule(ItemContainerType.Inventory, ItemContainerType.Equipment);
            Rule(ItemContainerType.Inventory, ItemContainerType.Weapons);
            Rule(ItemContainerType.Inventory, ItemContainerType.NanomachineAugmentations);
            Rule(ItemContainerType.Inventory, ItemContainerType.Quickslots, null, MoveQuickslots);
            Rule(ItemContainerType.Inventory, ItemContainerType.Storage);
            Rule(ItemContainerType.Inventory, ItemContainerType.Destroy, null, DestroyItems);
            Rule(ItemContainerType.Inventory, ItemContainerType.TransportStorage);

            Rule(ItemContainerType.Equipment, ItemContainerType.Inventory);
            Rule(ItemContainerType.Equipment, ItemContainerType.Storage);
            Rule(ItemContainerType.Equipment, ItemContainerType.Destroy, null, DestroyItems);
            Rule(ItemContainerType.Equipment, ItemContainerType.TransportStorage);

            Rule(ItemContainerType.Weapons, ItemContainerType.Inventory);
            Rule(ItemContainerType.Weapons, ItemContainerType.Weapons, null, MoveWeaponBetweenSlots);
            Rule(ItemContainerType.Weapons, ItemContainerType.Storage);
            Rule(ItemContainerType.Weapons, ItemContainerType.Destroy, null, DestroyItems);
            Rule(ItemContainerType.Weapons, ItemContainerType.TransportStorage);

            Rule(ItemContainerType.NanomachineAugmentations, ItemContainerType.Inventory);

            Rule(ItemContainerType.Quickslots, ItemContainerType.Quickslots, null, MoveQuickslots);

            Rule(ItemContainerType.Storage, ItemContainerType.Inventory);
            Rule(ItemContainerType.Storage, ItemContainerType.Equipment);
            Rule(ItemContainerType.Storage, ItemContainerType.Weapons);
            Rule(ItemContainerType.Storage, ItemContainerType.Destroy, null, DestroyItems);
            Rule(ItemContainerType.Storage, ItemContainerType.TransportStorage);

            Rule(ItemContainerType.Loot, ItemContainerType.Inventory);
            Rule(ItemContainerType.Loot, ItemContainerType.Equipment);
            Rule(ItemContainerType.Loot, ItemContainerType.Weapons);
            Rule(ItemContainerType.Loot, ItemContainerType.Storage);
            Rule(ItemContainerType.Loot, ItemContainerType.Destroy, null, DestroyItems);
            Rule(ItemContainerType.Loot, ItemContainerType.TransportStorage);

            Rule(ItemContainerType.TransportStorage, ItemContainerType.Inventory);
            Rule(ItemContainerType.TransportStorage, ItemContainerType.Storage);
            Rule(ItemContainerType.TransportStorage, ItemContainerType.Destroy, null, DestroyItems);

            Rule(ItemContainerType.TransportBuyback, ItemContainerType.Destroy, null, DestroyItems);
            Rule(ItemContainerType.TransportBuyback, ItemContainerType.TransportStorage);

            return rules;
        }

        private string? ValidateItemContainers(Player player, in MoveItems p)
        {
            static bool IsValidSlot(ItemContainerType containerType, ItemSlotType slotType)
            {
                return containerType switch
                {
                    ItemContainerType.Inventory => slotType == ItemSlotType.None,
                    ItemContainerType.Weapons => slotType is >= ItemSlotType.WeaponStart and < ItemSlotType.WeaponEnd,
                    ItemContainerType.Equipment => slotType is >= ItemSlotType.EquipmentStart and < ItemSlotType.EquipmentEnd,
                    ItemContainerType.Quickslots => slotType is >= ItemSlotType.QuickslotStart and < ItemSlotType.QuickslotEnd,
                    _ => true,
                };
            }

            if (!IsValidSlot(p.From, p.FromSlot))
            {
                return $"item container {p.From} has no slot {p.FromSlot}";
            }

            if (!IsValidSlot(p.To, p.ToSlot))
            {
                return $"item container {p.To} has no slot {p.ToSlot}";
            }

            return null;
        }

        private bool MoveBetweenContainers(Player player, in MoveItems p)
        {
            var fromContainer = GetItemContainer(player, p.From, p.FromSlot);
            var toContainer = GetItemContainer(player, p.To, p.ToSlot);
            if (!fromContainer.TryTransfer(toContainer, out _, p.ItemIds))
            {
                return false;
            }

            return true;
        }

        private bool MoveWeaponBetweenSlots(Player player, in MoveItems p)
        {
            var fromContainer = GetItemContainer(player, p.From, p.FromSlot);
            var toContainer = GetItemContainer(player, p.To, p.ToSlot);
            if (!fromContainer.TryTransferAll(toContainer, out _))
            {
                return false;
            }

            return true;
        }

        private bool MoveQuickslots(Player player, in MoveItems p)
        {
            // Into quickslots has an Item ID

            // Between quickslots has no item ID

            // Out of quickslots has no item ID

            // So basically, if an item moves into a quickslot it has an item otherwise it just has a slothe same container, the client expects the server to
            // infer the item's ID from the slot in question.
            return false;
        }

        private bool DestroyItems(Player player, in MoveItems p)
        {
            var fromContainer = GetItemContainer(player, p.From, p.FromSlot);
            if (!fromContainer.TryRemove(out _, p.ItemIds))
            {
                return false;
            }

            return true;
        }

        private ItemContainer GetItemContainer(Player player, ItemContainerType containerType, ItemSlotType slotType)
        {
            IItemLocation? location = null;
            if (containerType is ItemContainerType.Inventory or ItemContainerType.Weapons or ItemContainerType.Equipment)
            {
                location = player.Inventory;
            }

            if (location is null)
            {
                throw new ArgumentException($"Item container {containerType} has no associated location");
            }

            var container = location.GetItemContainer(slotType);
            if (container is null)
            {
                var locationRef = location.LocationRef;
                throw new ArgumentException($"Item container {containerType} does not exist in location {locationRef.Type} / {locationRef.Id}");
            }

            return container;
        }

        private readonly record struct TransferKey(ItemContainerType From, ItemContainerType To);

        private sealed record TransferRule(TransferValidator? Validate = null, TransferExecutor? Execute = null);
    }
}
