namespace FOMServer.Shared.Core
{
    /// <summary>
    /// A service for keeping track of the application's current lifecycle state.
    /// </summary>
    public interface IShutdownManager
    {
        /// <summary>
        /// The root token for the application.
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// A task that completes when the application starts stopping.
        /// </summary>
        Task Stopping { get; }

        /// <summary>
        /// A task that completes when the application has finished shutting down.
        /// </summary>
        Task Stopped { get; }

        /// <summary>
        /// Records a task that should be waited on during shutdown.
        /// </summary>
        void TrackTask(Task task);

        /// <summary>
        /// Starts the process so that the server can gracefully shut down.
        /// </summary>
        void StartShutdown();

        /// <summary>
        /// Shuts the server down gracefully.
        /// </summary>
        Task Shutdown();
    }
}
