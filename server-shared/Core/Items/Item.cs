using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Structs.Item;

namespace FOMServer.Shared.Core.Items
{
    public delegate void ItemDestroyedHandler(Item item);

    public sealed class ItemDestroyedException : InvalidOperationException
    {
        public ItemDestroyedException(Item item)
            : base($"ItemInterop {item.Id} has been destroyed") { }
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

        private readonly ushort _valueMax;
        private readonly byte _durabilityLossFactor;
        private readonly ItemSecurity _security;
        private readonly ItemRarity _rarity;
        private readonly uint _creatorPlayerId;
        private readonly uint _stolenFromPlayerId;
        private readonly uint _timeout;
        private readonly byte _attributeBonus;
        private readonly byte _recipeVariation;
        private readonly byte[] _recipeBalanceValues = new byte[BufferSizes.NumItemBalanceSliders];

        public Item(
            uint id,
            ItemType type,
            ItemLocationType locationType,
            uint locationId,
            ItemSlotType slot,
            ushort value,
            ushort durability,
            ushort durabilityMax,
            byte durabilityLossFactor,
            ItemSecurity security,
            ItemRarity rarity,
            uint creatorPlayerId,
            uint stolenFromPlayerId,
            uint timeout,
            byte attributeBonus,
            byte recipeVariation,
            ReadOnlySpan<byte> recipeBalanceValues
        )
        {
            Id = id;
            Type = type;

            _locationType = locationType;
            _locationId = locationId;
            _slot = slot;
            _value = value;
            _durability = durability;
            _valueMax = durabilityMax;
            _durabilityLossFactor = durabilityLossFactor;
            _security = security;
            _rarity = rarity;
            _creatorPlayerId = creatorPlayerId;
            _stolenFromPlayerId = stolenFromPlayerId;
            _timeout = timeout;
            _attributeBonus = attributeBonus;
            _recipeVariation = recipeVariation;

            if (recipeBalanceValues.Length != BufferSizes.NumItemBalanceSliders)
            {
                throw new ArgumentException(
                    nameof(recipeBalanceValues),
                    $"Received {recipeBalanceValues.Length} balance values, expected {BufferSizes.NumItemBalanceSliders}"
                );
            }
            for (var i = 0; i < recipeBalanceValues.Length; i++)
            {
                if (recipeBalanceValues[i] > ItemConstants.RecipeBalanceSliderMax)
                {
                    throw new ArgumentException(
                        nameof(recipeBalanceValues),
                        $"Balance slider {i} has a value {recipeBalanceValues[i]} above the maximum of {ItemConstants.RecipeBalanceSliderMax}"
                    );
                }

                _recipeBalanceValues[i] = recipeBalanceValues[i];
            }

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
                    $"ItemInterop {this} does not belong (location={locationRef.Type}, locationId={locationRef.Id}, slot={slot})",
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

        public unsafe void WriteTo(ref ItemInterop p)
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
                p.Base.ValueMax = _valueMax;
                p.Base.Durability = _durability;
                p.Base.DurabilityLossFactor = _durabilityLossFactor;
                p.Base.Security = _security;
                p.Base.Rarity = _rarity;
                p.Base.CreatorPlayerId = _creatorPlayerId;
                p.Base.StolenFromPlayerId = _stolenFromPlayerId;
                p.Base.Timeout = _timeout;
                p.Base.AttributeBonus = _attributeBonus;
                p.Base.RecipeVariation = _recipeVariation;
                for (var i = 0; i < BufferSizes.NumItemBalanceSliders; ++i)
                {
                    p.Base.RecipeBalanceValues[i] = _recipeBalanceValues[i];
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
