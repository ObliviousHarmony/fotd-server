using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Items;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    // Depletes the firing player's loaded rounds. The client sends ID_WEAPONFIRE
    // per shot and waits for the server to report the new ammo count; the payload
    // itself is not needed (the server tracks the weapon), so it is ignored.
    [PacketHandler]
    internal class WeaponFireHandler : PacketHandlerBase<WeaponFire>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IItemCatalog _itemCatalog;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<WeaponFireHandler> _logger;

        public WeaponFireHandler(
            IPlayerRegistry playerRegistry,
            IItemCatalog itemCatalog,
            IClientPacketSender clientPacketSender,
            ILogger<WeaponFireHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _itemCatalog = itemCatalog;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in WeaponFire p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received weapon fire from unregistered client '{Sender}'", sender);
                return;
            }

            var inventory = player.SnapshotInventory();
            if (!AmmoSupport.TryFindWeapon(inventory, _itemCatalog, out var weapon))
            {
                return;
            }

            // Empty magazine: nothing to deplete (the client shouldn't be firing).
            if (weapon.Base.Value == 0)
            {
                return;
            }

            var newValue = (ushort)(weapon.Base.Value - 1);
            if (!player.TrySetItemValue(weapon.Id, newValue, out var updated))
            {
                return;
            }

            _logger.LogDebug("Player {PlayerId} fired weapon {Type}, {Rounds} rounds left", player.Id, weapon.Base.Type, newValue);

            Span<Item> changed = [updated];
            AmmoSupport.SendItemsChanged(_clientPacketSender, sender, player.Id, changed);
        }
    }
}
