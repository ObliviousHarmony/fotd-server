using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums.Item;
using FOMServer.Shared.Core.Persistence;
using PacketItem = FOMServer.Shared.Core.Packets.Types.Item.Item;

namespace FOMServer.Shared.Core.Items
{
    public delegate void ItemDestroyedHandler(Item item);

    public sealed class ItemDestroyedException : InvalidOperationException
    {
        public ItemDestroyedException(Item item)
            : base($"Item {item.Id} has been destroyed") { }
    }

    public class Item : IPersistable
    {
        private readonly Lock _syncRoot = new();

        private ItemLocationType _locationType;
        private uint _locationId;
        private ItemSlotType _slot;
        private ushort _value;
        private ushort _durability;

        private IItemLocation? _location;
        private bool _destroyed;

        private readonly ushort _maxDurability;
        private readonly byte _durabilityLossFactor;

        public Item(
            uint id,
            ItemType type,
            ItemLocationType locationType,
            uint locationId,
            ItemSlotType slot,
            ushort value,
            ushort durability,
            ushort maxDurability,
            byte durabilityLossFactor
        )
        {
            Id = id;
            Type = type;

            _locationType = locationType;
            _locationId = locationId;
            _slot = slot;
            _value = value;
            _durability = durability;
            _maxDurability = maxDurability;
            _durabilityLossFactor = durabilityLossFactor;

            _destroyed = false;
        }

        public event PersistableChangeCallback? PersistableChange;

        public event ItemDestroyedHandler? ItemDestroyed;

        public uint Id { get; }

        public ItemType Type { get; }

        public ItemSlotType Slot
        {
            get
            {
                lock (_syncRoot)
                {
                    return _slot;
                }
            }
        }

        public void BindLocation(IItemLocation location, ItemSlotType slot)
        {
            lock (_syncRoot)
            {
                ThrowIfDestroyed();

                var locationRef = location.LocationRef;
                if (locationRef.Type == _locationType && locationRef.Id == _locationId && slot == _slot)
                {
                    _location = location;
                    return;
                }

                throw new ArgumentException(
                    $"Item {this} does not belong (location={locationRef.Type}, locationId={locationRef.Id}, slot={slot})",
                    nameof(location)
                );
            }
        }

        public void ChangeLocation(IItemLocation? newLocation, ItemSlotType newSlot)
        {
            IPersistable? oldLocationPersistable;
            IPersistable? newLocationPersistable;
            lock (_syncRoot)
            {
                ThrowIfDestroyed();

                oldLocationPersistable = _location?.LocationRef.Persistable;

                if (newLocation is not null)
                {
                    var newLoc = newLocation.LocationRef;

                    _locationType = newLoc.Type;
                    _locationId = newLoc.Id;
                    _location = newLocation;

                    newLocationPersistable = _location?.LocationRef.Persistable;
                }
                else
                {
                    _locationType = ItemLocationType.None;
                    _locationId = 0;
                    _location = null;

                    newLocationPersistable = null;
                }

                _slot = newSlot;
            }

            PersistableChange?.Invoke(this, newLocationPersistable, oldLocationPersistable);
        }

        public void SetValue(ushort newValue)
        {
            IPersistable? locationPersistable;
            lock (_syncRoot)
            {
                ThrowIfDestroyed();

                _value = newValue;

                locationPersistable = _location?.LocationRef.Persistable;
            }

            PersistableChange?.Invoke(this, locationPersistable);
        }

        public int UseValue(ushort numUses, bool decreaseDurability = false)
        {
            IPersistable? locationPersistable;
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

                locationPersistable = _location?.LocationRef.Persistable;
            }

            if (shouldDestroy)
            {
                Destroy();
            }
            else
            {
                PersistableChange?.Invoke(this, locationPersistable);
            }

            return uses;
        }

        public void ApplyDurabilityLoss(ushort durabilityLoss)
        {
            IPersistable? locationPersistable;
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

                locationPersistable = _location?.LocationRef.Persistable;
            }

            if (shouldDestroy)
            {
                Destroy();
            }
            else
            {
                PersistableChange?.Invoke(this, locationPersistable);
            }
        }

        public bool ShouldDropOnDeath()
        {
            lock (_syncRoot)
            {
                ThrowIfDestroyed();
            }

            return false;
        }

        public void WriteTo(ref PacketItem p)
        {
            lock (_syncRoot)
            {
                if (_destroyed)
                {
                    return;
                }

                p.Id = Id;
                p.Base.Type = Type;
                p.Base.Value = _value;
                p.Base.MaxDurability = _maxDurability;
                p.Base.Durability = _durability;
                p.Base.DurabilityLossFactor = _durabilityLossFactor;

                p.Base.Security = ItemSecurity.Normal;
                p.Base.CreatorPlayerId = 0;
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
            }
        }

        public override string ToString()
        {
            lock (_syncRoot)
            {
                return $"{Type} - {Id} (location={_locationType}, locationId={_locationId}, slot={_slot})";
            }
        }

        private bool Destroy()
        {
            IPersistable? locationPersistable;
            lock (_syncRoot)
            {
                if (_destroyed)
                {
                    return false;
                }

                _destroyed = true;

                locationPersistable = _location?.LocationRef.Persistable;
            }

            ItemDestroyed?.Invoke(this);
            PersistableChange?.Invoke(this, locationPersistable);

            return true;
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
