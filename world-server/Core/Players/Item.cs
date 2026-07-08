using System.Text;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Persistence;
using PacketItem = FOMServer.Shared.Core.Packets.Types.Item;

namespace FOMServer.World.Core.Players
{
    internal delegate void ItemDestroyedCallback(Item item);

    internal sealed class ItemDestroyedException : InvalidOperationException
    {
        public ItemDestroyedException(Item item)
            : base($"Item {item.Id} has been destroyed")
        {
        }
    }

    internal class Item : IPersistable
    {
        private readonly Lock _syncRoot = new();

        private Player? _owner;
        private uint _ownerId;
        private ushort _value;
        private ushort _durability;
        private bool _destroyed;

        private readonly ushort _maxDurability;
        private readonly byte _durabilityLossFactor;

        public Item(
            uint id,
            ItemType type,
            uint ownerId,
            ItemLocation location,
            uint locationId,
            ushort value,
            ushort durability,
            ushort maxDurability,
            byte durabilityLossFactor)
        {
            Id = id;
            Type = type;

            _ownerId = ownerId;
            Location = location;
            LocationId = locationId;
            _value = value;
            _durability = durability;

            _maxDurability = maxDurability;
            _durabilityLossFactor = durabilityLossFactor;

            _destroyed = false;
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public event ItemDestroyedCallback? OnDestroyed;

        public uint Id { get; }

        public ItemType Type { get; }

        public ItemLocation Location { get; private set; }

        public uint LocationId { get; private set; }

        public void BindOwner(Player owner)
        {
            lock (_syncRoot)
            {
                if (owner.Id != _ownerId)
                {
                    throw new ArgumentException($"Item {Id} cannot bind player {owner.Id}, expected player {_ownerId}", nameof(owner));
                }

                _owner = owner;
            }
        }

        public bool BelongsTo(Player owner)
        {
            lock (_syncRoot)
            {
                return ReferenceEquals(owner, _owner);
            }
        }

        public bool BelongsTo(uint ownerId)
        {
            lock (_syncRoot)
            {
                return ownerId == _ownerId;
            }
        }

        public bool BelongsIn(ItemLocation location, uint? locationId = null)
        {
            lock (_syncRoot)
            {
                if (_destroyed)
                {
                    throw new ItemDestroyedException(this);
                }

                if (locationId is not null && locationId != LocationId)
                {
                    return false;
                }

                return location == Location;
            }
        }

        public void Move(Player? newOwner, ItemLocation newLocation, uint newLocationId)
        {
            Player? oldOwner;
            lock (_syncRoot)
            {
                ThrowIfDestroyed();

                oldOwner = _owner;

                if (newOwner is not null)
                {
                    _ownerId = newOwner.Id;
                }

                _owner = newOwner;
                Location = newLocation;
                LocationId = newLocationId;
            }

            OnPersistableChange?.Invoke(this, newOwner, oldOwner);
        }

        public void SetValue(ushort newValue)
        {
            Player? owner;
            lock (_syncRoot)
            {
                ThrowIfDestroyed();

                _value = newValue;

                owner = _owner;
            }

            OnPersistableChange?.Invoke(this, owner);
        }

        public int UseValue(ushort numUses, bool decreaseDurability = false)
        {
            Player? owner;
            ushort uses;
            var shouldDestroy = false;
            lock (_syncRoot)
            {
                ThrowIfDestroyed();

                uses = Math.Min(_value, numUses);
                if (uses == 0)
                {
                    return 0;
                }

                _value -= Math.Min(_value, uses);

                if (decreaseDurability)
                {
                    _durability -= (ushort)Math.Min(_durability, Math.Ceiling(uses * _durabilityLossFactor / 100.0));
                    if (_durability == 0)
                    {
                        shouldDestroy = true;
                    }
                }

                owner = _owner;
            }

            if (shouldDestroy)
            {
                Destroy();
            }

            OnPersistableChange?.Invoke(this, owner);

            return uses;
        }

        public void ApplyDurabilityLoss(ushort durabilityLoss)
        {
            Player? owner;
            var shouldDestroy = false;
            lock (_syncRoot)
            {
                ThrowIfDestroyed();

                var loss = (ushort)Math.Min(_durability, Math.Ceiling(durabilityLoss * _durabilityLossFactor / 100.0));
                if (loss == 0)
                {
                    return;
                }

                _durability -= loss;
                if (_durability == 0)
                {
                    shouldDestroy = true;
                }

                owner = _owner;
            }

            if (shouldDestroy)
            {
                Destroy();
            }

            OnPersistableChange?.Invoke(this, owner);
        }

        public bool ShouldDropOnDeath()
        {
            lock (_syncRoot)
            {
                ThrowIfDestroyed();
            }

            return false;
        }

        public bool WriteTo(ref PacketItem p)
        {
            lock (_syncRoot)
            {
                if (_destroyed)
                {
                    return false;
                }

                p.Id = Id;
                p.Base.Type = Type;
                p.Base.Value = _value;
                p.Base.MaxDurability = _maxDurability;
                p.Base.Durability = _durability;
                p.Base.DurabilityLossFactor = _durabilityLossFactor;

                p.Base.Security = ItemSecurity.Normal;
                p.Base.CreatorPlayerId = _owner?.Id ?? 0;
                p.Base.Timeout = 0;
                p.Base.StolenFromPlayerId = 0;
                p.Base.Classification = 1;
                p.Base.Quality = ItemQuality.Standard;
                p.Base.AttributeBonus = 0;

                unsafe
                {
                    for (var i = 0; i < BufferSizes.NumItemBalanceSliders; ++i)
                    {
                        p.Base.BalanceValues[i] = 0;
                    }
                }

                return true;
            }
        }

        public override string ToString()
        {
            uint ownerId;
            ItemLocation location;
            uint locationId;

            lock (_syncRoot)
            {
                ownerId = _ownerId;
                location = Location;
                locationId = LocationId;
            }

            return $"{Type} - {Id} (owner={ownerId}, location={location}, locationId={locationId})";
        }

        private void Destroy()
        {
            lock (_syncRoot)
            {
                if (_destroyed)
                {
                    return;
                }
                _destroyed = true;
            }

            OnDestroyed?.Invoke(this);
        }

        private void ThrowIfDestroyed()
        {
            lock (_syncRoot)
            {
                if (_destroyed)
                {
                    throw new ItemDestroyedException(this);
                }
            }
        }
    }
}
