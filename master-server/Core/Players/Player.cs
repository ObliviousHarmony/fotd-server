using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Master.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly ClientSession _session;

        public Player(uint id, ClientSession session)
        {
            ID = id;
            _session = session;
        }

        public event PersistableChangeCallback? OnPersistableChange;

        public uint ID { get; }
    }
}
