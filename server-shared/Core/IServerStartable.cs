namespace FOMServer.Shared.Core
{
    /// <summary>
    /// A service that should be started once the server is ready.
    /// </summary>
    public interface IServerStartable
    {
        /// <summary>
        /// Called when the server is ready.
        /// </summary>
        void Start();
    }
}
