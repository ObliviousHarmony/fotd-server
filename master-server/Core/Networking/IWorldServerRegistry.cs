using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Core.Networking
{
    public interface IWorldServerRegistry
    {
        WorldServer[] GetAll();
        WorldServer? Get(WorldID id);
        WorldServer? Get(NetworkAddress address);
        WorldServer? Register(WorldID id, NetworkAddress serverAddress, NetworkAddress clientAddress);
        bool Unregister(WorldID id);
    }
}
