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

        public PlayerAttributes(Player player, uint[] values)
        {
            _player = player;

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

            public readonly AttributeMetadata Metadata => s_metadata[(int)_attribute];

            private uint Value
            {
                readonly get => _parent._values[(int)_attribute];
                set
                {
                    var v = Math.Min(value, Metadata.Max);
                    if (v == _parent._values[(int)_attribute])
                    {
                        return;
                    }

                    _parent._values[(int)_attribute] = v;
                    _changed = true;
                }
            }

            public readonly uint Get()
            {
                return Value;
            }

            public void Set(uint value)
            {
                Value = value;
            }

            public uint Change(int delta)
            {
                Value = (uint)Math.Max(Value + delta, 0L);
                return Value;
            }

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
                    _parent.PersistableChange?.Invoke(_parent, _parent._player);
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
