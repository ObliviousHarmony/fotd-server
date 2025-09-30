using FOMServer.Master.Core.Models;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Master.Application.Services
{
    public interface IWorldServerService
    {
        /// <summary>
        /// Registers a world server with the master server.
        /// </summary>
        WorldServer? Register(WorldID id, string clientAddress, ushort clientPort);

        /// <summary>
        /// Unregisters a world server from the master server.
        /// </summary>
        bool Unregister(WorldID id);
    }
}
