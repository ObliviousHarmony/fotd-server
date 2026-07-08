using System.Text;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Persistence;

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
        private ItemLocation _location;
        private uint _locationId;
        private ushort _value;
        private ushort _durability;
        private bool _destroyed;

        private readonly ushort _maxDurability;
        private readonly byte _durabilityLossFactor;
        private readonly ItemSecurity _security;
        private readonly uint _timeout;
        private readonly byte _classification;
        private readonly ItemQuality _quality;
        private readonly byte _attributeBonus;
        private readonly byte[] _balanceValues = new byte[BufferSizes.NumItemBalanceSliders];

        public Item(
            uint id,
            ItemType type,
            Player? owner,
            ItemLocation location,
            uint locationId,
            ushort value,
            ushort durability,
            ushort maxDurability,
            byte durabilityLossFactor,
            ItemSecurity security,
            uint timeout,
            byte classification,
            ItemQuality quality,
            byte attributeBonus,
            ReadOnlySpan<byte> balanceValues
        )
        {
            Id = id;
            Type = type;

            _owner = owner;
            _location = location;
            _locationId = locationId;
            _value = value;
            _durability = durability;

            _maxDurability = maxDurability;
            _durabilityLossFactor = durabilityLossFactor;
            _security = security;
            _timeout = timeout;
            _classification = classification;
            _quality = quality;
            _attributeBonus = attributeBonus;

            for (var i = 0; i < balanceValues.Length && i < _balanceValues.Length; i++)
            {
                _balanceValues[i] = balanceValues[i];
            }

            _destroyed = false;
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public event ItemDestroyedCallback? OnDestroyed;

        public uint Id { get; }

        public ItemType Type { get; }

        public bool BelongsIn(Player? player, ItemLocation location, uint? locationId = null)
        {
            lock (_syncRoot)
            {
                if (_destroyed)
                {
                    throw new ItemDestroyedException(this);
                }

                if (locationId is not null && _locationId != locationId)
                {
                    return false;
                }

                return ReferenceEquals(_owner, player) && _location == location;
            }
        }

        public void ChangeOwner(Player? newOwner, ItemLocation newLocation, uint newLocationId)
        {
            Player? oldOwner;
            lock (_syncRoot)
            {
                ThrowIfDestroyed();

                oldOwner = _owner;

                _owner = newOwner;
                _location = newLocation;
                _locationId = newLocationId;
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

        public override string ToString()
        {
            Player? owner;
            ItemLocation location;
            uint locationId;

            lock (_syncRoot)
            {
                owner = _owner;
                location = _location;
                locationId = _locationId;
            }

            return $"{Type} - {Id} (owner={owner?.Id.ToString() ?? "none"}, location={location}, locationId={locationId})";
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
