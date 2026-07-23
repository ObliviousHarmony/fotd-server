using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Interop.FOMNetwork.Structs;
using FOMServer.World.Core.World;

namespace FOMServer.World.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly Lock _syncRoot = new();

        private WorldUpdateInterop.CharacterUpdate _currentUpdate;

        public Player(uint id, uint[] attributes, IDictionary<uint, Item> inventory, ReadOnlySpan<ItemType> quickslots)
        {
            Id = id;
            _currentUpdate.Id = id;

            Position = new ServerPosition();
            Attributes = new PlayerAttributes(this, attributes);
            Inventory = new PlayerInventory(this, inventory);
            Quickslots = new PlayerQuickslots(this, quickslots);
        }

        public event PersistableChangeCallback? PersistableChange;

        public uint Id { get; }

        public NetworkAddress Address { get; private set; } = NetworkAddress.Unassigned;

        public ServerPosition Position { get; }

        public PlayerAttributes Attributes { get; }

        public PlayerInventory Inventory { get; }

        public PlayerQuickslots Quickslots { get; }

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

        public void ApplyUpdate(in WorldUpdateInterop.PlayerUpdate update)
        {
            lock (_syncRoot)
            {
                _currentUpdate = update.Character;
                _currentUpdate.Id = Id;

                Position.ApplyUpdate(update.Character.Position);
            }
        }

        public void WriteTo(ref WorldUpdateInterop p)
        {
            lock (_syncRoot)
            {
                p.Kind = WorldUpdateInterop.Type.Character;
                p.Character = _currentUpdate;
                Position.WriteTo(ref p.Character.Position);
            }
        }

        public void WriteTo(ref RegisterClientReturnPacket p)
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
                Inventory.WriteTo(ref p.Inventory, ref p.Weapons, ref p.Equipment);
                Quickslots.WriteTo(ref p.Quickslots);
            }
        }
    }
}
