using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.World.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly Lock _syncRoot = new();

        private readonly Lock _currentUpdateLock = new();
        private WorldUpdate _currentUpdate;

        public Player(uint id, int[]? initialAttributes = null)
        {
            Id = id;
            Attributes = new PlayerAttributes(this, initialAttributes);

            _currentUpdate.Id = id;
            _currentUpdate.Kind = WorldUpdate.Type.Character;
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

        public void ApplyUpdate(in WorldUpdate update)
        {
            lock (_currentUpdateLock)
            {
                _currentUpdate = update;

                _currentUpdate.Id = Id;
                _currentUpdate.Kind = WorldUpdate.Type.Character;
            }
        }

        public WorldUpdate CaptureUpdate()
        {
            lock (_currentUpdateLock)
            {
                return _currentUpdate;
            }
        }
    }
}
