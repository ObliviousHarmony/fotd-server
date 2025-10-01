namespace FOMServer.Shared.Core.Networking
{
    public interface IServerService
    {
        /// <summary>
        /// Starts a server interface on the specified port.
        /// </summary>
        /// <returns>A pointer to the server interface, 0 if there was an error.</returns>
        nint Startup(ushort port);

        /// <summary>
        /// Shuts down the server interface.
        /// </summary>
        void Shutdown(nint server);
    }
}
