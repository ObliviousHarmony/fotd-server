using FOMServer.Shared.Interop.FOMNetwork.Enums;

namespace FOMServer.Shared.Core.Constants
{
    public static class ServerConstants
    {
        /// <summary>
        /// The version of the Face of Mankind client that this server supports.
        /// </summary>
        public const int ClientVersion = 1853;

        /// <summary>
        /// The constant that the Face of Mankind client sends on ID_WORLD_LOGIN.
        /// </summary>
        public const int WorldLoginConstant = 1293394;

        /// <summary>
        /// The port that the master server listens for world server connections on.
        /// </summary>
        public const ushort MasterWorldPort = 61100;

        /// <summary>
        /// The port that the master server listens for client connections on.
        /// </summary>
        public const ushort MasterClientPort = 61000;

        /// <summary>
        /// Gets the port that the world server should listen for client connections on.
        /// </summary>
        public static ushort GetWorldClientPort(WorldId worldId)
        {
            return (ushort)(MasterClientPort + (ushort)worldId);
        }
    }
}
