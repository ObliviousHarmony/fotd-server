using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Core.Items
{
    /// <summary>
    /// The server's representation of an item.
    /// </summary>
    /// <remarks>
    /// This class intentionally has no lock. The expectation is that it should ONLY ever be modified
    /// under the lock of the container that holds the item. This avoids potential scenarios where
    /// an item lock might be contended unintentionally.
    /// </remarks>
    internal class Item : IPersistable
    {
        private readonly byte[] _balanceValues = new byte[BufferSizes.NumItemBalanceSliders];

        public event PersistableChangeCallback? OnPersistableChange;

        public uint Id { get; }

        public ItemLocation Location { get; private set; }

        public Player? Owner { get; private set; }

        public uint LocationId { get; private set; }

        public ItemType Type { get; }

        public ushort Value { get; private set; }

        public ushort MaxDurability { get; }

        public ushort Durability { get; private set; }

        public byte DurabilityLossFactor { get; }

        public ItemSecurity Security { get; }

        public uint Timeout { get; }

        public byte Classification { get; }

        public ItemQuality Quality { get; }

        public byte AttributeBonus { get; }

        public ReadOnlySpan<byte> BalanceValues => _balanceValues;

        public void ChangeOwner(Player? newOwner, ItemLocation newLocation, uint newLocationId)
        {
            var oldOwner = Owner;

            Owner = newOwner;
            Location = newLocation;
            LocationId = newLocationId;

            OnPersistableChange!(this, Owner, oldOwner);
        }
    }
}
