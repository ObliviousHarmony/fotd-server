namespace FOMServer.Shared.Core
{
    /// <summary>
    /// A service that should be started once the server is ready.
    /// </summary>
    public interface IServerStartable
    {
        void Start();
    }
}
