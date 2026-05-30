using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

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
