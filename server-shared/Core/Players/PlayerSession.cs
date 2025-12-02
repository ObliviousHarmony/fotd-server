using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Core.Players
{
    /// <summary>
    /// Represents an active player session, serving as the identity anchor
    /// for persistence associations across all player-owned state.
    /// </summary>
    public class PlayerSession : IPersistable
    {
        public PlayerSession(uint id, NetworkAddress clientAddress)
        {
            ID = id;
            ClientAddress = clientAddress;
        }

        public event PersistenceChangedHandler? OnPersistableChange;

        public uint ID { get; }
        public NetworkAddress ClientAddress { get; }
    }
}
