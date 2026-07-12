using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Master.Core.Players
{
    internal class Player : IPersistable
    {
        private readonly ClientSession _session;

        public Player(uint id, ClientSession session)
        {
            Id = id;
            _session = session;
        }

        public event PersistableChangeCallback? PersistableChange;

        public uint Id { get; }
    }
}
