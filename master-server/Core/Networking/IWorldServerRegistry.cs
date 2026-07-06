using FOMServer.Shared.Core.Enums;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

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
