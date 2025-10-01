namespace FOMServer.Shared.Core.Networking
{
    public interface IClientService
    {
        /// <summary>
        /// Connects to a remote server.
        /// </summary>
        /// <returns>A pointer to the opened connection, 0 if there was a failure.</returns>
        nint Connect(string hostAddress, ushort port);

        /// <summary>
        /// Disconnects from a remote server.
        /// </summary>
        void Disconnect(nint client);
    }
}
