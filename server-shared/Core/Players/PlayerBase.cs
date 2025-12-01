using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Core.Players
{
    /// <summary>
    /// Base class for player entities.
    /// </summary>
    public abstract class PlayerBase : IPersistable
    {
        public uint ID { get; init; }
        public NetworkAddress ClientAddress { get; set; }

        public event PersistenceChangedHandler? OnPersistableChange;
    }
}
