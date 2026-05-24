using FOMServer.Shared.Core.Persistence;

namespace FOMServer.World.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly ClientSession _session;

        public Player(uint id, ClientSession session, int[]? initialAttributes = null)
        {
            ID = id;
            _session = session;
            Attributes = new PlayerAttributes(this, initialAttributes);
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public uint ID { get; }

        public PlayerAttributes Attributes { get; }
    }
}
