using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.World.Core.Items;
using FOMServer.World.Core.Networking;

namespace FOMServer.World.Application.Handlers
{
    /// <summary>
    /// Shared helpers for the weapon-fire / reload handlers: locating the player's
    /// weapon and matching ammo in their (flat, session) inventory, and pushing an
    /// <see cref="ItemsChanged"/> update back to the client.
    /// </summary>
    /// <remarks>
    /// v1 simplification: the server has no equipment-slot model yet, so "the
    /// weapon" is the first weapon-category item in the backpack. This is correct
    /// while a player carries a single weapon (the common case); multi-weapon
    /// disambiguation needs equipment tracking (equip flows through ID_MOVE_ITEMS).
    /// </remarks>
    internal static class AmmoSupport
    {
        // dest the client's ID_ITEMS_REMOVED handler treats as "splice these ids out
        // of PLAYERDATA_INVENTORY and refresh the backpack" (FUN_10192d40 dest==3 →
        // FUN_1023f120 + dirty flag). dest==1 there is a different path (a global
        // item-id lookup that doesn't cover backpack items and never refreshes).
        private const byte InventoryRemoveDest = 3;

        public static bool TryFindWeapon(Item[] inventory, IItemCatalog catalog, out Item weapon)
        {
            foreach (var item in inventory)
            {
                if (catalog.IsWeapon((ushort)item.Base.Type))
                {
                    weapon = item;
                    return true;
                }
            }

            weapon = default;
            return false;
        }

        public static bool TryFindLoadableAmmo(Item[] inventory, ushort ammoType, out Item clip)
        {
            foreach (var item in inventory)
            {
                if ((ushort)item.Base.Type == ammoType && item.Base.Value > 0)
                {
                    clip = item;
                    return true;
                }
            }

            clip = default;
            return false;
        }

        public static void SendItemsChanged(
            IClientPacketSender sender,
            NetworkAddress destination,
            uint playerId,
            ReadOnlySpan<Item> items)
        {
            using var response = new PacketWriter<ItemsChanged>(destination);
            ref var data = ref response.Data;
            data.PlayerId = playerId;

            var count = Math.Min(items.Length, ItemsChanged.MaxItems);
            data.Count = (byte)count;
            for (var i = 0; i < count; i++)
            {
                data.Items[i] = items[i];
            }

            sender.Send(response.Build());
        }

        public static void SendItemRemoved(
            IClientPacketSender sender,
            NetworkAddress destination,
            uint playerId,
            uint itemId)
        {
            using var response = new PacketWriter<ItemsRemoved>(destination);
            ref var data = ref response.Data;
            data.PlayerId = playerId;
            data.Dest = InventoryRemoveDest;
            data.IdCount = 1;
            data.Ids[0] = itemId;

            sender.Send(response.Build());
        }
    }
}
