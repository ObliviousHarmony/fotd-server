using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Master.Core.Networking
{
    public interface IWorldServerService
    {
        /// <summary>
        /// Gets all of the world servers currently registered with the master server.
        /// </summary>
        WorldServer[] GetAll();

        /// <summary>
        /// Gets a world server by its ID.
        /// </summary>
        WorldServer? Get(WorldID id);

        /// <summary>
        /// Gets a world server by its network address.
        /// </summary>
        WorldServer? Get(NetworkAddress address);

        /// <summary>
        /// Registers a world server with the master server.
        /// </summary>
        WorldServer? Register(WorldID id, NetworkAddress serverAddress, NetworkAddress clientAddress);

        /// <summary>
        /// Unregisters a world server from the master server.
        /// </summary>
        bool Unregister(WorldID id);
    }
}
