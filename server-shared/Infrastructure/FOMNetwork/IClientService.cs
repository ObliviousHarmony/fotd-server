namespace FOMServer.Shared.Infrastructure.FOMNetwork
{
    public interface IClientService
    {
        /// <summary>
        /// Connects to a remote server.
        /// </summary>
        /// <param name="hostAddress">The domain or IP to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <returns>A pointer to the opened connection, 0 if there was a failure.</returns>
        IntPtr Connect(string hostAddress, ushort port);

        /// <summary>
        /// Disconnects from a remote server.
        /// </summary>
        /// <param name="client">The client to disconnect.</param>
        void Disconnect(IntPtr client);
    }
}
