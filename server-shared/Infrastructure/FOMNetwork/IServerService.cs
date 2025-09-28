namespace FOMServer.Shared.Infrastructure.FOMNetwork
{
    public interface IServerService
    {
        /// <summary>
        /// Starts a server interface on the specified port.
        /// </summary>
        /// <returns>A pointer to the server interface, 0 if there was an error.</returns>
        IntPtr Startup(ushort port);

        /// <summary>
        /// Shuts down the server interface.
        /// </summary>
        void Shutdown(IntPtr server);
    }
}
