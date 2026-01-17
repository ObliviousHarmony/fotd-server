using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Shared.Core.Player
{
    public interface IPlayerRegistryBase<TPlayer> where TPlayer : PlayerBase
    {
        TPlayer? Get(uint id);
        TPlayer? Get(NetworkAddress clientAddress);
        TPlayer? Register(uint id, NetworkAddress clientAddress);
        void Unregister(uint id);
    }
}
