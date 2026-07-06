using FOMServer.Shared.Core.Persistence;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;
using WorldUpdate = FOMServer.Shared.Core.Packets.Types.WorldUpdate;

namespace FOMServer.World.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly Lock _syncRoot = new();

        private readonly Lock _currentUpdateLock = new();
        private WorldUpdate.CharacterUpdate _currentUpdate;

        public Player(uint id, uint[]? initialAttributes = null)
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
