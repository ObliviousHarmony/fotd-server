using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Core.Players
{
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
