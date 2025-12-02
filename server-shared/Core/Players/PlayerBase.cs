using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Core.Players
{
    /// <summary>
    /// Base class for player entities.
    /// </summary>
    public abstract class PlayerBase : IPersistable
    {
        private readonly uint _id;
        private readonly NetworkAddress _clientAddress;

        protected PlayerBase(uint id, NetworkAddress clientAddress)
        {
            _id = id;
            _clientAddress = clientAddress;
        }

        public event PersistenceChangedHandler? OnPersistableChange;

        public uint ID => _id;
        public NetworkAddress ClientAddress => _clientAddress;
    }
}
