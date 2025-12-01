using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Players
{
    /// <summary>
    /// Base class for player entities.
    /// </summary>
    public abstract class PlayerBase
    {
        public uint ID { get; init; }
        public NetworkAddress ClientAddress { get; set; }
    }
}
