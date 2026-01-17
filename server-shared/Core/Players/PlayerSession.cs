using FOMServer.Shared.Core.Packets.Types;
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

        public event PersistableChangeCallback? OnPersistableChange;

        public uint ID { get; }
        public NetworkAddress ClientAddress { get; }
    }
}
