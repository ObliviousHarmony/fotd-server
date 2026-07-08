using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;
using PacketWorldUpdate = FOMServer.Shared.Core.Packets.Types.WorldUpdate;
using RegisterClientReturnPacket = FOMServer.Shared.Core.Packets.RegisterClientReturn;

namespace FOMServer.World.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly Lock _syncRoot = new();

        private readonly Lock _currentUpdateLock = new();
        private PacketWorldUpdate.CharacterUpdate _currentUpdate;

        private readonly Item _tempItem;

        public Player(uint id, uint[]? initialAttributes = null)
        {
            Id = id;
            _currentUpdate.Id = id;
            Attributes = new PlayerAttributes(this, initialAttributes);

            Inventory = new ItemBag(this, ItemLocation.Inventory, 0, []);

            _tempItem = new Item(1, ItemType.Fedora, this, ItemLocation.Inventory, 0, 100, 1000, 1000, 100);
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public uint Id { get; }

        public NetworkAddress Address { get; private set; } = NetworkAddress.Unassigned;

        public PlayerAttributes Attributes { get; }

        public ItemBag Inventory { get; }

        public void ClaimForClient(NetworkAddress address)
        {
            lock (_syncRoot)
            {
                if (Address != NetworkAddress.Unassigned)
                {
                    throw new InvalidOperationException($"Client '{address}' cannot claim player {Id} ({Address})");
                }
                Address = address;
            }
        }

        public void ApplyUpdate(in PacketWorldUpdate.PlayerUpdate update)
        {
            lock (_currentUpdateLock)
            {
                _currentUpdate = update.Character;
                _currentUpdate.Id = Id;
            }
        }

        public bool WriteTo(ref PacketWorldUpdate packet)
        {
            lock (_currentUpdateLock)
            {
                packet.Kind = PacketWorldUpdate.Type.Character;
                packet.Character = _currentUpdate;
            }

            return true;
        }

        public bool WriteTo(ref RegisterClientReturnPacket packet)
        {
            lock (_syncRoot)
            {
                packet.PlayerId = Id;
                packet.Profile.PlayerName = "Naruto Uzumaki";

                packet.Avatar.Face = 5;
                packet.Avatar.Hair = 2;
                packet.Avatar.Shirt = 0;
                packet.Avatar.Bottoms = 0;
                packet.Avatar.Shoes = 0;

                Attributes.WriteTo(ref packet.Attributes);
                Inventory.WriteTo(ref packet.Inventory);

                _tempItem.WriteTo(ref packet.Equipment[(int)EquipmentSlot.Shirt]);
            }

            return true;
        }
    }
}
