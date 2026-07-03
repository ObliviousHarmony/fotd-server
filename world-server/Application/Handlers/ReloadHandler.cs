using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Items;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    // Reloads the player's weapon from a matching ammo clip. v1 dumps the whole
    // clip into the weapon (we have no per-weapon magazine capacity yet), consuming
    // the clip's rounds, and reports both items back via ID_ITEMS_CHANGED. The
    // ID_RELOAD payload is not needed (the server picks the weapon + clip).
    [PacketHandler]
    internal class ReloadHandler : PacketHandlerBase<Reload>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IItemCatalog _itemCatalog;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<ReloadHandler> _logger;

        public ReloadHandler(
            IPlayerRegistry playerRegistry,
            IItemCatalog itemCatalog,
            IClientPacketSender clientPacketSender,
            ILogger<ReloadHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _itemCatalog = itemCatalog;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in Reload p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received reload from unregistered client '{Sender}'", sender);
                return;
            }

            var inventory = player.SnapshotInventory();
            if (!AmmoSupport.TryFindWeapon(inventory, _itemCatalog, out var weapon))
            {
                return;
            }

            var ammoType = _itemCatalog.GetAmmoType((ushort)weapon.Base.Type);
            if (ammoType == 0)
            {
                return;
            }

            if (!AmmoSupport.TryFindLoadableAmmo(inventory, ammoType, out var clip))
            {
                _logger.LogDebug("Player {PlayerId} tried to reload {Type} with no ammo (type {AmmoType})", player.Id, weapon.Base.Type, ammoType);
                return;
            }

            // Already at least a full clip loaded — don't consume a clip, but still
            // confirm the weapon's current ammo. During rapid fire the client's
            // loaded count lags the server and it can send a redundant reload; if we
            // answer with silence it shows a spurious "no ammunition" error, so
            // re-sync it with an ItemsChanged for the (unchanged) weapon instead.
            if (weapon.Base.Value >= clip.Base.Value)
            {
                _logger.LogDebug("[RELOAD] Player {PlayerId}: {Type} already full (loaded={Loaded}), confirming", player.Id, weapon.Base.Type, weapon.Base.Value);
                Span<Item> full = [weapon];
                AmmoSupport.SendItemsChanged(_clientPacketSender, sender, player.Id, full);
                return;
            }

            // Load one clip as a fresh magazine (the clip's rounds become the loaded
            // count) and consume that clip entirely. We have no per-weapon magazine
            // capacity yet, so one clip == one full magazine; a partial mag's leftover
            // rounds are discarded on swap.
            var loaded = clip.Base.Value;
            if (!player.TrySetItemValue(weapon.Id, loaded, out var updatedWeapon) ||
                !player.RemoveItem(clip.Id))
            {
                return;
            }

            _logger.LogInformation("Player {PlayerId} reloaded {Type} to {Rounds} rounds (clip {ClipId} consumed)", player.Id, weapon.Base.Type, loaded, clip.Id);

            Span<Item> changed = [updatedWeapon];
            AmmoSupport.SendItemsChanged(_clientPacketSender, sender, player.Id, changed);
            AmmoSupport.SendItemRemoved(_clientPacketSender, sender, player.Id, clip.Id);
        }
    }
}
