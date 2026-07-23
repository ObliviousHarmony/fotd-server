using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;

namespace FOMServer.Master.Core.Networking
{
    internal interface IWorldServerRegistry
    {
        WorldServer[] GetAll();

        WorldServer? Get(WorldId id);

        WorldId[] Register(WorldId[] ids, NetworkAddress serverAddress, NetworkAddress publicAddress);

        WorldId[] Unregister(NetworkAddress serverAddress);
    }
}
