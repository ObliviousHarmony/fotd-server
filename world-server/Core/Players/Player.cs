using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.World.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly Lock _syncRoot = new();

        public Player(uint id, int[]? initialAttributes = null)
        {
            ID = id;
            Attributes = new PlayerAttributes(this, initialAttributes);
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public uint ID { get; }

        public NetworkAddress Address { get; private set; } = NetworkAddress.Unassigned;

        public PlayerAttributes Attributes { get; }

        public void ClaimForClient(NetworkAddress address)
        {
            lock (_syncRoot)
            {
                if (Address != NetworkAddress.Unassigned)
                {
                    throw new InvalidOperationException($"Client '{address}' cannot claim player {ID} ({Address})");
                }
                Address = address;
            }
        }
    }
}
