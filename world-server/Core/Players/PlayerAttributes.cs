using System.Diagnostics;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Exceptions;

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
        private readonly int[] _values = new int[AttributeCount];
        private readonly int[] _locks = new int[AttributeCount];

        static PlayerAttributes()
        {
            s_metadata = new AttributeMetadata[(int)AttributeType.NUM_ATTRIBUTE_TYPES];
            for (var i = 0; i < s_metadata.Length; i++)
            {
                s_metadata[i] = new() { Max = 1000, Default = 0, LockRequired = false };
            }

            s_metadata[(int)AttributeType.Health] = new() { Max = 1000, Default = 1000, LockRequired = false };
            s_metadata[(int)AttributeType.Stamina] = new() { Max = 1000, Default = 1000, LockRequired = false };
            s_metadata[(int)AttributeType.BioEnergy] = new() { Max = 1000, Default = 1000, LockRequired = false };
            s_metadata[(int)AttributeType.Aura] = new() { Max = 1000, Default = 1000, LockRequired = false };
            s_metadata[(int)AttributeType.Agility] = new() { Max = 1000, Default = 700, LockRequired = false };
            s_metadata[(int)AttributeType.UniversalCredits] = new() { Max = int.MaxValue, Default = 0, LockRequired = true };
            s_metadata[(int)AttributeType.FactionCredits] = new() { Max = int.MaxValue, Default = 0, LockRequired = true };
            s_metadata[(int)AttributeType.Coins] = new() { Max = int.MaxValue, Default = 0, LockRequired = true };
        }

        public PlayerAttributes(Player player, int[]? initialValues = null)
        {
            _player = player;

            initialValues?.CopyTo(_values, 0);
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public uint PlayerID => _player.ID;

        public static ref readonly AttributeMetadata GetMetadata(AttributeType attribute)
        {
            return ref s_metadata[(int)attribute];
        }

        public static ReadOnlySpan<AttributeMetadata> GetAllMetadata()
        {
            return s_metadata;
        }

        /// <summary>
        /// Gets the current value of an attribute, clamped to [0, Max].
        /// </summary>
        public uint Get(AttributeType attribute)
        {
            var index = (int)attribute;
            return (uint)Math.Clamp(Volatile.Read(ref _values[index]), 0, s_metadata[index].Max);
        }

        /// <summary>
        /// Sets an attribute to an absolute value.
        /// </summary>
        /// <remarks>
        /// Throws if the attribute requires locking and spins if it's currently locked.
        /// </remarks>
        public void Set(AttributeType attribute, uint value)
        {
            var index = (int)attribute;
            ref readonly var metadata = ref s_metadata[index];

            if (metadata.LockRequired)
            {
                throw new InvalidOperationException(
                    $"{attribute} requires locking. Use Lock() to acquire exclusive access");
            }

            // Wait for the attribute to unlock. There is a small window between
            // this spin completing and the write below where a Lock() could acquire
            // and write, then be overwritten by our Volatile.Write. This is acceptable
            // because Set is used for infrequent, derived values (e.g., armor from
            // equipment).
            while (Volatile.Read(ref _locks[index]) != 0)
            {
                Thread.SpinWait(1);
            }

            Volatile.Write(ref _values[index], Math.Min((int)value, metadata.Max));
            _ = (OnPersistableChange?.Invoke(this, _player));
        }

        /// <summary>
        /// Atomically changes an attribute by a delta.
        /// </summary>
        /// <remarks>
        /// Throws if the attribute requires locking and spins if it's currently locked.
        /// </remarks>
        /// <param name="attribute">The attribute to modify.</param>
        /// <param name="delta">The amount to change (negative to subtract).</param>
        public uint Change(AttributeType attribute, int delta)
        {
            var index = (int)attribute;
            ref readonly var metadata = ref s_metadata[index];

            if (metadata.LockRequired)
            {
                throw new InvalidOperationException(
                    $"{attribute} requires locking. Use Lock() to acquire exclusive access");
            }

            // Wait for the attribute to unlock. There is a small window between
            // this spin completing and the write below where a Lock() could acquire
            // and write, then be overwritten by our Volatile.Write. This is acceptable
            // because Set is used for infrequent, derived values (e.g., armor from
            // equipment).
            while (Volatile.Read(ref _locks[index]) != 0)
            {
                Thread.SpinWait(1);
            }

            var result = (uint)Math.Clamp(Interlocked.Add(ref _values[index], delta), 0, metadata.Max);
            _ = (OnPersistableChange?.Invoke(this, _player));
            return result;
        }

        /// <summary>
        /// Acquires an exclusive lock on an attribute.
        /// </summary>
        /// <param name="attribute">The attribute to lock.</param>
        /// <exception cref="AttributeDeadlockException">
        /// Thrown if the lock cannot be acquired within the timeout period.
        /// </exception>
        public LockedAttribute Lock(AttributeType attribute)
        {
            return new LockedAttribute(this, attribute);
        }

        /// <summary>
        /// Provides exclusive access to a locked attribute.
        /// </summary>
        public ref struct LockedAttribute
        {
            private readonly PlayerAttributes _parent;
            private readonly AttributeType _attribute;
            private bool _changed;
            private bool _disposed;

            public LockedAttribute(PlayerAttributes parent, AttributeType attribute)
            {
                _parent = parent;
                _attribute = attribute;
                _changed = false;
                _disposed = false;

                // Attempt to acquire the lock with a 100ms timeout to avoid deadlocks.
                var index = (int)attribute;
                var spinner = new SpinWait();
                var timeoutTimestamp = Stopwatch.GetTimestamp() + (Stopwatch.Frequency / 10);

                while (Interlocked.CompareExchange(ref _parent._locks[index], 1, 0) != 0)
                {
                    spinner.SpinOnce();

                    if (spinner.NextSpinWillYield && Stopwatch.GetTimestamp() > timeoutTimestamp)
                    {
                        throw new AttributeDeadlockException(attribute);
                    }
                }
            }

            /// <summary>
            /// Gets the current value, clamped to [0, Max].
            /// </summary>
            public readonly uint Get()
            {
                var index = (int)_attribute;
                return (uint)Math.Clamp(_parent._values[index], 0, s_metadata[index].Max);
            }

            /// <summary>
            /// Sets the attribute to an absolute value, clamped to [0, Max].
            /// </summary>
            public void Set(uint value)
            {
                var index = (int)_attribute;
                _parent._values[index] = Math.Min((int)value, s_metadata[index].Max);
                _changed = true;
            }

            /// <summary>
            /// Changes the attribute by a delta and clamps the result to [0, Max].
            /// </summary>
            public uint Change(int delta)
            {
                var index = (int)_attribute;
                var clamped = Math.Clamp(_parent._values[index] + delta, 0, s_metadata[index].Max);
                _parent._values[index] = clamped;
                _changed = true;
                return (uint)clamped;
            }

            /// <summary>
            /// Releases the lock on the attribute.
            /// </summary>
            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                Volatile.Write(ref _parent._locks[(int)_attribute], 0);

                if (_changed)
                {
                    _ = (_parent.OnPersistableChange?.Invoke(_parent, _parent._player));
                }
            }
        }

        public readonly struct AttributeMetadata
        {
            public int Max { get; init; }

            public int Default { get; init; }

            public bool LockRequired { get; init; }
        }
    }
}
