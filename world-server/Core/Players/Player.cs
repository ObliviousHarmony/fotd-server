using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
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
            ReadOnlySpan<ItemType> quickslots)
        {
            Id = id;
            _currentUpdate.Id = id;

            Attributes = new PlayerAttributes(this, attributes);
            Inventory = new PlayerInventory(this, inventory);
            Quickslots = new PlayerQuickslots(this, quickslots);
        }

        public event PersistableChangeCallback? PersistableChange;

        public uint Id { get; }

        public NetworkAddress Address { get; private set; } = NetworkAddress.Unassigned;

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

        public void ApplyUpdate(in PacketWorldUpdate.PlayerUpdate update)
        {
            lock (_syncRoot)
            {
                _currentUpdate = update.Character;
                _currentUpdate.Id = Id;
            }
        }

        public void WriteTo(ref PacketWorldUpdate p)
        {
            lock (_syncRoot)
            {
                p.Kind = PacketWorldUpdate.Type.Character;
                p.Character = _currentUpdate;
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
