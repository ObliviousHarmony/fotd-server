using System.Diagnostics;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Exceptions;
using PacketPlayerAttributes = FOMServer.Shared.Core.Packets.Types.PlayerAttributes;

namespace FOMServer.World.Core.Players
{
    /// <summary>
    /// Thread-safe storage for player attributes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most attributes support lock-free operations via <see cref="Get"/>,
    /// <see cref="Set"/>, and <see cref="Add"/>. These methods use atomic
    /// operations and spin briefly if the attribute is currently locked.
    /// </para>
    /// <para>
    /// For compound operations across multiple attributes, acquire locks
    /// on all involved attributes. Be mindful of lock ordering to avoid
    /// deadlocks - the <see cref="LockedAttribute"/> constructor will throw
    /// <see cref="AttributeDeadlockException"/> if acquisition times out.
    /// </para>
    /// </remarks>
    internal class PlayerAttributes : IPersistable
    {
        public const int AttributeCount = (int)AttributeType.NUM_ATTRIBUTE_TYPES;

        private static readonly AttributeMetadata[] s_metadata;

        private readonly Player _player;
        private readonly uint[] _values = new uint[AttributeCount];
        private readonly long[] _locks = new long[AttributeCount];
        private long _nextLockId;
        private bool _dirty;

        static PlayerAttributes()
        {
            s_metadata = new AttributeMetadata[AttributeCount];

            for (var id = 0; id < AttributeCount; id++)
            {
                var attribute = (AttributeType)id;

                var lockRequired = false;
                switch (attribute)
                {
                    case AttributeType.FactionCredits:
                    case AttributeType.UniversalCredits:
                    case AttributeType.Coins:
                        lockRequired = true;
                        break;
                }

                s_metadata[id] = new()
                {
                    Max = PlayerConstants.AttributeMaxValues[id],
                    Default = PlayerConstants.AttributeDefaultValues[id],
                    LockRequired = lockRequired,
                };
            }
        }

        public PlayerAttributes(Player player, uint[] values)
        {
            _player = player;
            _nextLockId = 0;
            _dirty = false;

            if (values.Length != AttributeCount)
            {
                throw new ArgumentException(
                    $"Expected {AttributeCount} values but got {values.Length}",
                    nameof(values)
                );
            }

            for (var i = 0; i < AttributeCount; i++)
            {
                var max = s_metadata[i].Max;
                if (values[i] > max)
                {
                    throw new ArgumentException(
                        $"Value for {(AttributeType)i} is {values[i]}, which exceeds max ({max})",
                        nameof(values)
                    );
                }

                _values[i] = values[i];
            }
        }

        public event PersistableChangeCallback? PersistableChange;

        public uint PlayerId => _player.Id;

        public static ref readonly AttributeMetadata GetMetadata(AttributeType attribute)
        {
            return ref s_metadata[(int)attribute];
        }

        public static ReadOnlySpan<AttributeMetadata> GetAllMetadata()
        {
            return s_metadata;
        }

        public uint Get(AttributeType attribute)
        {
            return Volatile.Read(ref _values[(int)attribute]);
        }

        public uint Change(AttributeType attribute, int delta)
        {
            var index = (int)attribute;
            ref readonly var metadata = ref s_metadata[index];

            if (metadata.LockRequired)
            {
                throw new InvalidOperationException($"Changing {attribute} requires a lock");
            }

            uint current,
                updated;
            do
            {
                // Defer to an active LockedAttribute holder before reading. If a Lock()
                // acquires and writes after this spin, our CompareExchange below fails
                // against the new value and we retry, so no update is ever clobbered.
                while (Volatile.Read(ref _locks[index]) != 0)
                {
                    Thread.SpinWait(1);
                }

                current = Volatile.Read(ref _values[index]);
                updated = (uint)Math.Clamp(current + delta, 0L, metadata.Max);

                if (updated == current)
                {
                    return current;
                }
            } while (Interlocked.CompareExchange(ref _values[index], updated, current) != current);

            PersistableChange?.Invoke(this, _player);
            return updated;
        }

        public void WriteTo(ref PacketPlayerAttributes p)
        {
            unsafe
            {
                for (var i = 0; i < (int)AttributeType.NUM_ATTRIBUTE_TYPES; ++i)
                {
                    p.Values[i] = Volatile.Read(ref _values[i]);
                }
            }
        }

        public LockedAttribute Lock(AttributeType attribute)
        {
            return new LockedAttribute(this, attribute, Interlocked.Increment(ref _nextLockId));
        }

        public LockedAttributes Lock(params ReadOnlySpan<AttributeType> attributes)
        {
            return new LockedAttributes(this, attributes, Interlocked.Increment(ref _nextLockId));
        }

        private void MarkDirty()
        {
            Interlocked.Exchange(ref _dirty, true);
        }

        private bool WasDirty()
        {
            return Interlocked.Exchange(ref _dirty, false);
        }

        private void AcquireLock(AttributeType attribute, long lockId)
        {
            var index = (int)attribute;

            var spinner = new SpinWait();
            var timeoutTimestamp = Stopwatch.GetTimestamp() + (Stopwatch.Frequency / 50);
            while (Interlocked.CompareExchange(ref _locks[index], lockId, 0) != 0)
            {
                spinner.SpinOnce();

                if (spinner.NextSpinWillYield &&
                    Stopwatch.GetTimestamp() > timeoutTimestamp)
                {
                    throw new AttributeDeadlockException(attribute);
                }
            }
        }

        private bool ReleaseLock(AttributeType attribute, long lockId)
        {
            return Interlocked.CompareExchange(ref _locks[(int)attribute], 0, lockId) == lockId;
        }

        /// <summary>
        /// Provides exclusive access to a locked attribute.
        /// </summary>
        internal readonly ref struct LockedAttribute
        {
            private readonly PlayerAttributes _parent;
            private readonly AttributeType _attribute;
            private readonly long _lockId;

            internal LockedAttribute(PlayerAttributes parent, AttributeType attribute, long lockId)
            {
                _parent = parent;
                _attribute = attribute;
                _lockId = lockId;

                _parent.AcquireLock(attribute, lockId);
            }

            public ref readonly AttributeMetadata Metadata => ref GetMetadata(_attribute);

            public readonly uint Value
            {
                get => _parent._values[(int)_attribute];
                set
                {
                    var newValue = Math.Min(value, Metadata.Max);

                    ref var current = ref _parent._values[(int)_attribute];
                    if (current == newValue)
                    {
                        return;
                    }

                    current = newValue;
                    _parent.MarkDirty();
                }
            }

            public readonly uint Change(int delta)
            {
                Value = (uint)Math.Max(Value + delta, 0L);
                return Value;
            }

            public readonly void Dispose()
            {
                if (!_parent.ReleaseLock(_attribute, _lockId))
                {
                    throw new InvalidOperationException($"Lost ownership of {_attribute} lock {_lockId}");
                }

                if (_parent.WasDirty())
                {
                    _parent.PersistableChange?.Invoke(_parent, _parent._player);
                }
            }
        }

        /// <summary>
        /// Provides exclusive access to multiple locked attributes.
        /// </summary>
        internal readonly ref struct LockedAttributes
        {
            private readonly PlayerAttributes _parent;
            private readonly ulong _lockedAttributesMask;
            private readonly long _lockId;

            internal LockedAttributes(
                PlayerAttributes parent,
                scoped ReadOnlySpan<AttributeType> attributes,
                long lockId
                )
            {
                _parent = parent;
                _lockId = lockId;

                foreach (var attribute in attributes)
                {
                    _lockedAttributesMask |= 1UL << (int)attribute;
                }

                var acquiredMask = 0UL;

                try
                {
                    // Always acquire in enum order to prevent deadlocks.
                    for (var i = 0; i < AttributeCount; i++)
                    {
                        if ((_lockedAttributesMask & (1UL << i)) == 0)
                        {
                            continue;
                        }

                        parent.AcquireLock((AttributeType)i, _lockId);
                        acquiredMask |= 1UL << i;
                    }
                }
                catch
                {
                    // Roll back any locks acquired before failure.
                    for (var i = AttributeCount - 1; i >= 0; i--)
                    {
                        if ((acquiredMask & (1UL << i)) == 0)
                        {
                            continue;
                        }

                        parent.ReleaseLock((AttributeType)i, _lockId);
                    }

                    throw;
                }
            }

            public readonly uint this[AttributeType attribute]
            {
                get
                {
                    EnsureLocked(attribute);
                    return _parent._values[(int)attribute];
                }

                set
                {
                    EnsureLocked(attribute);

                    ref readonly var metadata = ref GetMetadata(attribute);
                    var newValue = Math.Min(value, metadata.Max);

                    ref var current = ref _parent._values[(int)attribute];
                    if (current == newValue)
                    {
                        return;
                    }

                    current = newValue;
                    _parent.MarkDirty();
                }
            }

            public readonly uint Change(AttributeType attribute, int delta)
            {
                EnsureLocked(attribute);

                var value = (uint)Math.Clamp(
                    this[attribute] + delta,
                    0L,
                    GetMetadata(attribute).Max);

                this[attribute] = value;

                return value;
            }

            public readonly void Dispose()
            {
                for (var i = AttributeCount - 1; i >= 0; i--)
                {
                    if ((_lockedAttributesMask & (1UL << i)) == 0)
                    {
                        continue;
                    }

                    if (!_parent.ReleaseLock((AttributeType)i, _lockId))
                    {
                        throw new InvalidOperationException(
                            $"Lost ownership of {(AttributeType)i} lock.");
                    }
                }

                if (_parent.WasDirty())
                {
                    _parent.PersistableChange?.Invoke(
                        _parent,
                        _parent._player);
                }
            }

            private readonly void EnsureLocked(AttributeType attribute)
            {
                if ((_lockedAttributesMask & (1UL << (int)attribute)) == 0)
                {
                    throw new InvalidOperationException(
                        $"{attribute} was not locked.");
                }
            }
        }

        internal readonly struct AttributeMetadata
        {
            public uint Max { get; init; }

            public uint Default { get; init; }

            public bool LockRequired { get; init; }
        }
    }
}
