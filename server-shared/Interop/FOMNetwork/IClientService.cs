namespace FOMServer.Shared.Interop.FOMNetwork
{
    public interface IClientService
    {
        /// <summary>
        /// Connects to a remote server.
        /// </summary>
        /// <returns>A pointer to the opened connection, 0 if there was a failure.</returns>
        IntPtr Connect(string hostAddress, ushort port, int threadSleepMs);

        /// <summary>
        /// Disconnects from a remote server.
        /// </summary>
        void Disconnect(IntPtr client);
    }
}
