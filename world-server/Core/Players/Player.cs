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

        private PacketWorldUpdate.CharacterUpdate _currentUpdate;

        public Player(
            uint id,
            uint[] attributes,
            IDictionary<uint, Item> inventory,
            IDictionary<uint, Item> equipment,
            IDictionary<uint, Item> weapons,
            IDictionary<uint, Item> activeConsumables,
            IDictionary<uint, Item> nanomachineAugmentations)
        {
            Id = id;
            _currentUpdate.Id = id;

            Attributes = new PlayerAttributes(this, attributes);
            Inventory = new ItemBag(this, ItemLocation.Inventory, 0, inventory);
            Equipment = new PlayerEquipment(this, equipment);
            Weapons = new PlayerWeapons(this, weapons);
            ActiveConsumables = new PlayerActiveConsumables(this, activeConsumables);
            NanomachineAugmentations = new PlayerNanomachineAugmentations(this, nanomachineAugmentations);
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
            lock (_syncRoot)
            {
                _currentUpdate = update.Character;
                _currentUpdate.Id = Id;
            }
        }

        public bool WriteTo(ref PacketWorldUpdate p)
        {
            lock (_syncRoot)
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
