using FOMServer.Shared.Core.Enums;
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
    public sealed class PlayerAttributes
    {
        private const int DeadlockSpinThreshold = 10_000_000;

        private readonly struct AttributeMetadata
        {
            public int Max { get; init; }
            public bool LockRequired { get; init; }
        }

        private static readonly AttributeMetadata[] s_metadata;

        private readonly int[] _values;
        private readonly int[] _locks;

        static PlayerAttributes()
        {
            s_metadata = new AttributeMetadata[(int)PlayerAttribute.NUM_ATTRIBUTES];
            for (int i = 0; i < s_metadata.Length; i++)
                s_metadata[i] = new() { Max = 1000, LockRequired = false };

            s_metadata[(int)PlayerAttribute.UC] = new() { Max = int.MaxValue, LockRequired = true };
            s_metadata[(int)PlayerAttribute.FC] = new() { Max = int.MaxValue, LockRequired = true };
            s_metadata[(int)PlayerAttribute.Coins] = new() { Max = int.MaxValue, LockRequired = true };
            s_metadata[(int)PlayerAttribute.XP] = new() { Max = int.MaxValue, LockRequired = false };
        }

        public PlayerAttributes()
        {
            _values = new int[(int)PlayerAttribute.NUM_ATTRIBUTES];
            _locks = new int[(int)PlayerAttribute.NUM_ATTRIBUTES];
        }

        /// <summary>
        /// Gets the current value of an attribute, clamped to [0, Max].
        /// </summary>
        public uint Get(PlayerAttribute attribute)
        {
            int index = (int)attribute;
            return (uint)Math.Clamp(Volatile.Read(ref _values[index]), 0, s_metadata[index].Max);
        }

        /// <summary>
        /// Sets an attribute to an absolute value.
        /// </summary>
        /// <remarks>
        /// Throws if the attribute requires locking and spins if it's currently locked.
        /// </remarks>
        public void Set(PlayerAttribute attribute, uint value)
        {
            int index = (int)attribute;
            ref readonly var metadata = ref s_metadata[index];

            if (metadata.LockRequired)
                throw new InvalidOperationException(
                    $"{attribute} requires locking. Use Lock() to acquire exclusive access.");

            // Wait for the attribute to unlock.
            while (Volatile.Read(ref _locks[index]) != 0)
                Thread.SpinWait(1);

            Volatile.Write(ref _values[index], Math.Min((int)value, metadata.Max));
        }

        /// <summary>
        /// Atomically changes an attribute by a delta.
        /// </summary>
        /// <remarks>
        /// Throws if the attribute requires locking and spins if it's currently locked.
        /// </remarks>
        /// <param name="attribute">The attribute to modify.</param>
        /// <param name="delta">The amount to change (negative to subtract).</param>
        /// <returns>The clamped value after modification.</returns>
        public uint Change(PlayerAttribute attribute, int delta)
        {
            int index = (int)attribute;
            ref readonly var metadata = ref s_metadata[index];

            if (metadata.LockRequired)
                throw new InvalidOperationException(
                    $"{attribute} requires locking. Use Lock() to acquire exclusive access.");

            // Wait for the attribute to unlock.
            while (Volatile.Read(ref _locks[index]) != 0)
                Thread.SpinWait(1);

            return (uint)Math.Clamp(Interlocked.Add(ref _values[index], delta), 0, metadata.Max);
        }

        /// <summary>
        /// Acquires an exclusive lock on an attribute.
        /// </summary>
        /// <param name="attribute">The attribute to lock.</param>
        /// <returns>A handle providing exclusive access to the attribute.</returns>
        /// <exception cref="AttributeDeadlockException">
        /// Thrown if the lock cannot be acquired within the timeout period.
        /// </exception>
        public LockedAttribute Lock(PlayerAttribute attribute)
        {
            return new LockedAttribute(this, attribute);
        }

        /// <summary>
        /// Provides exclusive access to a locked attribute.
        /// </summary>
        public ref struct LockedAttribute
        {
            private readonly PlayerAttributes _parent;
            private readonly PlayerAttribute _attribute;
            private bool _disposed;

            internal LockedAttribute(PlayerAttributes parent, PlayerAttribute attribute)
            {
                _parent = parent;
                _attribute = attribute;
                _disposed = false;

                int index = (int)attribute;
                int spins = 0;

                while (Interlocked.CompareExchange(ref _parent._locks[index], 1, 0) != 0)
                {
                    if (++spins > DeadlockSpinThreshold)
                        throw new AttributeDeadlockException(attribute);

                    Thread.SpinWait(1);
                }
            }

            /// <summary>
            /// Gets the current value, clamped to [0, Max].
            /// </summary>
            public uint Get()
            {
                int index = (int)_attribute;
                return (uint)Math.Clamp(_parent._values[index], 0, s_metadata[index].Max);
            }

            /// <summary>
            /// Sets the attribute to an absolute value, clamped to [0, Max].
            /// </summary>
            public void Set(uint value)
            {
                int index = (int)_attribute;
                _parent._values[index] = Math.Min((int)value, s_metadata[index].Max);
            }

            /// <summary>
            /// Changes the attribute by a delta and clamps the result to [0, Max].
            /// </summary>
            /// <returns>The clamped value after modification.</returns>
            public uint Change(int delta)
            {
                int index = (int)_attribute;
                int clamped = Math.Clamp(_parent._values[index] + delta, 0, s_metadata[index].Max);
                _parent._values[index] = clamped;
                return (uint)clamped;
            }

            /// <summary>
            /// Releases the lock on the attribute.
            /// </summary>
            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                Volatile.Write(ref _parent._locks[(int)_attribute], 0);
            }
        }
    }
}
