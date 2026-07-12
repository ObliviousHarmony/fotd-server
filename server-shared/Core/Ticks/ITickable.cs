namespace FOMServer.Shared.Core.Ticks
{
    /// <summary>
    /// A unit of background work that the tick service runs periodically on the shared tick loop.
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// How often <see cref="TickAsync"/> should run. The scheduler treats this as the desired
        /// cadence; finer or wall-clock-accurate timing is the tickable's own concern.
        /// </summary>
        TimeSpan TickInterval { get; }

        /// <summary>
        /// Runs a single tick. Invoked on the shared tick loop, so heavy work here delays the
        /// other tickables.
        /// </summary>
        /// <param name="cancellationToken">
        /// Cancelled when the server is shutting down. After cancellation every tickable is invoked
        /// once more as a final shutdown sweep.
        /// </param>
        ValueTask TickAsync(CancellationToken cancellationToken);
    }
}
