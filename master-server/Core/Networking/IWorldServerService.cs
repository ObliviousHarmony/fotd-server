using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.Master.Core.Networking
{
    public interface IWorldServerService
    {
        /// <summary>
        /// Gets all of the world servers currently registered with the master server.
        /// </summary>
        WorldServer[] GetAll();

        /// <summary>
        /// Registers a world server with the master server.
        /// </summary>
        WorldServer? Register(WorldID id, NetworkAddress serverAddress, string clientAddress, ushort clientPort);

        /// <summary>
        /// Unregisters a world server from the master server.
        /// </summary>
        bool Unregister(WorldID id);
    }
}
