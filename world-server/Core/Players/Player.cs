using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.World.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly Lock _syncRoot = new();

        private readonly Lock _currentUpdateLock = new();
        private WorldUpdate.CharacterUpdate _currentUpdate;

        // Authoritative in-memory backpack. Session-scoped for now: mutations raise
        // OnPersistableChange so the future DB-backed inventory step captures them,
        // but nothing subscribes yet, so items only survive re-entry within a session.
        private readonly List<Item> _inventory = new();

        public Player(uint id, int[]? initialAttributes = null)
        {
            Id = id;
            Attributes = new PlayerAttributes(this, initialAttributes);

            _currentUpdate.Id = id;
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public uint Id { get; }

        public NetworkAddress Address { get; private set; } = NetworkAddress.Unassigned;

        public PlayerAttributes Attributes { get; }

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

        /// <summary>
        /// Adds an item to the player's backpack and signals the change for
        /// persistence.
        /// </summary>
        public void AddItem(in Item item)
        {
            lock (_syncRoot)
            {
                _inventory.Add(item);
            }

            OnPersistableChange?.Invoke(this);
        }

        /// <summary>
        /// Returns a snapshot copy of the player's backpack for packet building.
        /// </summary>
        public Item[] SnapshotInventory()
        {
            lock (_syncRoot)
            {
                return _inventory.ToArray();
            }
        }

        /// <summary>
        /// Sets the <see cref="ItemBase.Value"/> of the backpack item with the
        /// given instance id (e.g. a weapon's loaded rounds or a clip's remaining
        /// rounds) and returns the updated item. Returns false if no such item.
        /// </summary>
        public bool TrySetItemValue(uint itemId, ushort value, out Item updated)
        {
            updated = default;
            var changed = false;
            lock (_syncRoot)
            {
                for (var i = 0; i < _inventory.Count; i++)
                {
                    if (_inventory[i].Id != itemId)
                    {
                        continue;
                    }

                    var item = _inventory[i];
                    item.Base.Value = value;
                    _inventory[i] = item;
                    updated = item;
                    changed = true;
                    break;
                }
            }

            if (changed)
            {
                OnPersistableChange?.Invoke(this);
            }

            return changed;
        }

        /// <summary>
        /// Removes the backpack item with the given instance id (e.g. a clip fully
        /// consumed by a reload). Returns false if no such item.
        /// </summary>
        public bool RemoveItem(uint itemId)
        {
            var removed = false;
            lock (_syncRoot)
            {
                for (var i = 0; i < _inventory.Count; i++)
                {
                    if (_inventory[i].Id == itemId)
                    {
                        _inventory.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
            }

            if (removed)
            {
                OnPersistableChange?.Invoke(this);
            }

            return removed;
        }

        public void ApplyUpdate(in WorldUpdate.PlayerUpdate update)
        {
            lock (_currentUpdateLock)
            {
                _currentUpdate = update.Character;
                _currentUpdate.Id = Id;
            }
        }

        public WorldUpdate.CharacterUpdate CaptureUpdate()
        {
            lock (_currentUpdateLock)
            {
                return _currentUpdate;
            }
        }
    }
}
