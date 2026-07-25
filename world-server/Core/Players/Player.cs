using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Interop.FOMNetwork.Structs;
using FOMServer.World.Core.World;

namespace FOMServer.World.Core.Players
{
    internal class Player : IPersistable, IPersistableProvider
    {
        private volatile string _name;

        private readonly Lock _currentUpdateLock = new();
        private WorldUpdateInterop.CharacterUpdate _currentUpdate;

        public Player(
            uint id,
            string name,
            AvatarConstants.Sex sex,
            AvatarConstants.Race race,
            ushort face,
            ushort hair,
            uint[] attributes,
            IDictionary<uint, Item> inventory,
            IDictionary<uint, Item> equipment,
            ReadOnlySpan<ItemType> quickslots
        )
        {
            Id = id;
            _name = name;
            _currentUpdate.Id = id;

            Address = NetworkAddress.Unassigned;
            Position = new ServerPosition();
            Avatar = new PlayerAvatar(this, sex, race, face, hair);
            Attributes = new PlayerAttributes(this, attributes);
            Inventory = new PlayerInventory(this, inventory);
            Equipment = new PlayerEquipment(this, equipment);
            Quickslots = new PlayerQuickslots(this, quickslots);

            Equipment.EquipmentChanged += (_, snapshots) => Avatar.UpdateEquipment(snapshots);
            Avatar.UpdateEquipment(Equipment.ToSnapshots());
        }

        public event PersistableChangeHandler? PersistableChange;

        public uint Id { get; }

        public string Name
        {
            get => _name;
            private set => _name = value;
        }

        // Deliberately left unlocked. It's write-once, set before the player is published and so any reader that obtains a player
        // from the registry is guaranteed to see the complete write.
        public NetworkAddress Address { get; private set; }

        public ServerPosition Position { get; }
        public PlayerAvatar Avatar { get; }
        public PlayerAttributes Attributes { get; }
        public PlayerInventory Inventory { get; }
        public PlayerEquipment Equipment { get; }
        public PlayerQuickslots Quickslots { get; }

        public void ClaimForClient(NetworkAddress address)
        {
            if (Address != NetworkAddress.Unassigned)
            {
                throw new InvalidOperationException($"Client '{address}' cannot claim player {Id} ({Address})");
            }
            Address = address;
        }

        public void CollectPersistables(ICollection<IPersistable> destination)
        {
            destination.Add(this);
            Inventory.CollectPersistables(destination);
            Equipment.CollectPersistables(destination);
            destination.Add(Attributes);
            destination.Add(Quickslots);
        }

        public void ApplyUpdate(in WorldUpdateInterop.PlayerUpdate update)
        {
            lock (_currentUpdateLock)
            {
                _currentUpdate = update.Character;
                _currentUpdate.Id = Id;

                Position.ApplyUpdate(update.Character.Position);
            }
        }

        public void WriteTo(ref WorldUpdateInterop p)
        {
            lock (_currentUpdateLock)
            {
                p.Kind = WorldUpdateInterop.Type.Character;
                p.Character = _currentUpdate;
                Avatar.WriteTo(ref p.Character.Avatar);
                Position.WriteTo(ref p.Character.Position);
            }
        }

        public void WriteTo(ref RegisterClientReturnPacket p)
        {
            p.PlayerId = Id;
            p.Profile.PlayerName = _name;

            Avatar.WriteTo(ref p.Avatar);
            Attributes.WriteTo(ref p.Attributes);
            Inventory.WriteTo(ref p.Inventory);
            Equipment.WriteTo(ref p.Weapons, ref p.Equipment);
            Quickslots.WriteTo(ref p.Quickslots);
        }
    }
}
