using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Core.Networking
{
    public interface IWorldServerRegistry
    {
        WorldServer[] GetAll();
        WorldServer? Get(WorldID id);
        WorldID[] Register(WorldID[] ids, NetworkAddress serverAddress, NetworkAddress publicAddress);
        WorldID[] Unregister(NetworkAddress serverAddress);
    }
}
