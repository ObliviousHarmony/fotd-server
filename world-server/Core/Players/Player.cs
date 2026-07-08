using System.Xml.Linq;
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

        public Player(uint id, uint[]? initialAttributes = null)
        {
            Id = id;
            _currentUpdate.Id = id;
            Attributes = new PlayerAttributes(this, initialAttributes);

            // ======================================================
            // DEVELOPMENT TESTING STUFF
            // ======================================================
            var itemIdOffset = id * 1000;
            Attributes.Change(AttributeType.Stamina, 10000);
            Item[] tempInv = [
                new Item(itemIdOffset + 1, ItemType.Zanathid5Inflex, this, ItemLocation.Inventory, 0, 100, 1000, 1000, 100)
            ];
            var tempEq = new Dictionary<EquipmentSlot, Item>() {
                {EquipmentSlot.Hat, new Item(itemIdOffset + 2, ItemType.Fedora, this, ItemLocation.Equipment, (uint)EquipmentSlot.Hat, 100, 1000, 1000, 100) },
                {EquipmentSlot.Back, new Item(itemIdOffset + 3, ItemType.ShieldAugmentation, this, ItemLocation.Equipment, (uint)EquipmentSlot.Back, 100, 1000, 1000, 100) },
                {EquipmentSlot.Eyes, new Item(itemIdOffset + 4, ItemType.AlmDesignsGlassesBlack, this, ItemLocation.Equipment, (uint)EquipmentSlot.Eyes, 100, 1000, 1000, 100) }
            };
            var tempWep = new Dictionary<uint, Item>() {
                {0, new Item(itemIdOffset + 5, ItemType.DOA187, this, ItemLocation.Weapons, 0, 100, 1000, 1000, 100) }
            };
            var tempConsume = new Dictionary<uint, Item>() {
                {0, new Item(itemIdOffset + 6, ItemType.DoublecheeseMystique, this, ItemLocation.ActiveConsumable, 0, 100, 1000, 1000, 100) }
            };
            var tempAug = new Dictionary<uint, Item>() {
                {0, new Item(itemIdOffset + 7, ItemType.ElectromyographicRegulator, this, ItemLocation.NanomachineAugmentation, 0, 100, 1000, 1000, 100) }
            };

            // ======================================================

            Inventory = new ItemBag(this, ItemLocation.Inventory, 0, tempInv);
            Equipment = new PlayerEquipment(this, tempEq);
            Weapons = new PlayerWeapons(this, tempWep);
            ActiveConsumables = new PlayerActiveConsumables(this, tempConsume);
            NanomachineAugmentations = new PlayerNanomachineAugmentations(this, tempAug);
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public uint Id { get; }

        public NetworkAddress Address { get; private set; } = NetworkAddress.Unassigned;

        public PlayerAttributes Attributes { get; }

        public ItemBag Inventory { get; }

        public PlayerEquipment Equipment { get; }

        public PlayerWeapons Weapons { get; }

        public PlayerActiveConsumables ActiveConsumables { get; }

        public PlayerNanomachineAugmentations NanomachineAugmentations { get; }

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

        public bool WriteTo(ref PacketWorldUpdate p)
        {
            lock (_currentUpdateLock)
            {
                p.Kind = PacketWorldUpdate.Type.Character;
                p.Character = _currentUpdate;
            }

            return true;
        }

        public bool WriteTo(ref RegisterClientReturnPacket p)
        {
            lock (_syncRoot)
            {
                p.PlayerId = Id;
                p.Profile.PlayerName = "Naruto Uzumaki";

                p.Avatar.Face = 5;
                p.Avatar.Hair = 2;
                p.Avatar.Shirt = 0;
                p.Avatar.Bottoms = 0;
                p.Avatar.Shoes = 0;

                Attributes.WriteTo(ref p.Attributes);
                Inventory.WriteTo(ref p.Inventory);
                Equipment.WriteTo(ref p.Equipment);
                Weapons.WriteTo(ref p.Weapons);
                ActiveConsumables.WriteTo(ref p.ActiveConsumables);
                NanomachineAugmentations.WriteTo(ref p.NanomachineAugmentations);
            }

            return true;
        }
    }
}
