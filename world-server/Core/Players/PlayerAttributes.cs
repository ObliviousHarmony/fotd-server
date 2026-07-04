using System.Diagnostics;
using FOMServer.Shared.Core.Constants;
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
        private readonly uint[] _values = new uint[AttributeCount];
        private readonly int[] _locks = new int[AttributeCount];

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

        public PlayerAttributes(Player player, uint[]? initialValues = null)
        {
            _player = player;

            initialValues?.CopyTo(_values, 0);
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public uint PlayerId => _player.Id;

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
            return Math.Min(Volatile.Read(ref _values[index]), s_metadata[index].Max);
        }

        /// <summary>
        /// Atomically changes an attribute by a delta, clamped to [0, Max].
        /// </summary>
        /// <remarks>
        /// Uses a compare-and-swap loop so the stored value is always kept within
        /// bounds. A delta that would breach a bound is clamped before it is written,
        /// so it can never leave phantom under/overflow behind for a later change to
        /// pay back. A change that clamps to a no-op neither writes nor raises
        /// <see cref="OnPersistableChange"/>.
        ///
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

            uint current, updated;
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

            OnPersistableChange?.Invoke(this, _player);
            return updated;
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

                // Attempt to acquire the lock with a 20ms timeout to avoid deadlocks.
                var index = (int)attribute;
                var spinner = new SpinWait();
                var timeoutTimestamp = Stopwatch.GetTimestamp() + (Stopwatch.Frequency / 50);

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
                return Math.Min(_parent._values[index], s_metadata[index].Max);
            }

            /// <summary>
            /// Sets the attribute to an absolute value, clamped to [0, Max].
            /// </summary>
            public void Set(uint value)
            {
                var index = (int)_attribute;
                _parent._values[index] = Math.Min(value, s_metadata[index].Max);
                _changed = true;
            }

            /// <summary>
            /// Changes the attribute by a delta and clamps the result to [0, Max].
            /// </summary>
            public uint Change(int delta)
            {
                var index = (int)_attribute;
                var clamped = (uint)Math.Clamp(_parent._values[index] + delta, 0L, s_metadata[index].Max);
                _parent._values[index] = clamped;
                _changed = true;
                return clamped;
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
                    _parent.OnPersistableChange?.Invoke(_parent, _parent._player);
                }
            }
        }

        public readonly struct AttributeMetadata
        {
            public uint Max { get; init; }

            public uint Default { get; init; }

            public bool LockRequired { get; init; }
        }
    }
}
