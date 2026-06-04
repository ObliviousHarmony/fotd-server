using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Ticking;

namespace FOMServer.Shared.Application.Ticking
{
    /// <summary>
    /// Drives every registered <see cref="ITickable"/> from a single shared loop. The loop wakes
    /// on a fixed base period (the smallest registered interval) and counts wake-ups to decide
    /// which tickables are due, staggering equal-interval tickables so they don't all fire on the
    /// same wake-up. Tickables run serially, so a heavy tick delays the rest. A tickable is
    /// expected never to throw; an escaped exception triggers a graceful server shutdown.
    /// </summary>
    internal class ServerTickService : IServerStartable
    {
        /// <summary>
        /// The fastest the loop will ever wake, regardless of how small a tick interval is.
        /// </summary>
        private static readonly TimeSpan s_minBasePeriod = TimeSpan.FromMilliseconds(1);

        private readonly IShutdownManager _shutdownManager;
        private readonly ILogger<ServerTickService> _logger;
        private readonly TimeSpan _basePeriod;
        private readonly ScheduledTickable[] _scheduled;

        private Task? _loopTask;
        private CancellationTokenSource? _cts;

        public ServerTickService(
            IEnumerable<ITickable> tickables,
            IShutdownManager shutdownManager,
            ILogger<ServerTickService> logger)
        {
            _shutdownManager = shutdownManager;
            _logger = logger;

            var registered = tickables.ToArray();
            if (registered.Length == 0)
            {
                _scheduled = [];
                return;
            }

            var fastest = registered.Min(t => t.TickInterval);
            _basePeriod = fastest < s_minBasePeriod ? s_minBasePeriod : fastest;
            _scheduled = BuildSchedule(registered, _basePeriod);
        }

        /// <summary>
        /// Starts the shared tick loop.
        /// </summary>
        public void Start()
        {
            if (_loopTask is not null || _scheduled.Length == 0)
            {
                return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownManager.Token);

            // A persistent loop wants its own thread rather than tying up a pool thread.
            _loopTask = Task.Factory.StartNew(
                async () => await TickLoopAsync(_cts.Token),
                _cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            ).Unwrap();

            // Make sure that the shutdown manager waits for this task to complete.
            _shutdownManager.TrackTask(_loopTask);
        }

        /// <summary>
        /// Advances the schedule by one base-period wake-up, collecting the tickables that are due
        /// this wake-up into <paramref name="due"/>. Separated from the loop so the scheduling
        /// decision can be tested without a timer.
        /// </summary>
        internal void TickOnce(List<ITickable> due)
        {
            due.Clear();

            for (var i = 0; i < _scheduled.Length; i++)
            {
                if (--_scheduled[i].Countdown > 0)
                {
                    continue;
                }

                _scheduled[i].Countdown = _scheduled[i].TicksPerRun;
                due.Add(_scheduled[i].Tickable);
            }
        }

        /// <summary>
        /// Main loop that wakes once per base period and runs the tickables that are due.
        /// </summary>
        private async Task TickLoopAsync(CancellationToken ct)
        {
            using var timer = new PeriodicTimer(_basePeriod);
            var due = new List<ITickable>(_scheduled.Length);

            try
            {
                while (await timer.WaitForNextTickAsync(ct))
                {
                    TickOnce(due);

                    foreach (var tickable in due)
                    {
                        try
                        {
                            await tickable.TickAsync(ct);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            // Tickables are expected to handle their own failures; an escaped
                            // exception is an unexpected, unrecoverable fault, so bring the server
                            // down gracefully rather than continue in an unknown state.
                            _logger.LogCritical(ex, "Unhandled exception in {Tickable}; shutting down", tickable.GetType().Name);
                            _shutdownManager.StartShutdown();
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static ScheduledTickable[] BuildSchedule(ITickable[] tickables, TimeSpan basePeriod)
        {
            var scheduled = new ScheduledTickable[tickables.Length];
            var groupCounts = new Dictionary<int, int>();

            for (var i = 0; i < tickables.Length; i++)
            {
                var ticksPerRun = Math.Max(1, (int)Math.Round(tickables[i].TickInterval / basePeriod));

                // Spread tickables that share an interval across [1 .. ticksPerRun] by their order
                // within that interval group, so equal-interval tickables land on different
                // wake-ups instead of bunching onto the same one.
                var groupIndex = groupCounts.GetValueOrDefault(ticksPerRun);
                groupCounts[ticksPerRun] = groupIndex + 1;

                scheduled[i] = new ScheduledTickable
                {
                    Tickable = tickables[i],
                    TicksPerRun = ticksPerRun,
                    Countdown = 1 + (groupIndex % ticksPerRun)
                };
            }

            return scheduled;
        }

        /// <summary>
        /// A tickable bundled with its scheduling state. <see cref="Countdown"/> counts base-period
        /// wake-ups down to the next run, then resets to <see cref="TicksPerRun"/>.
        /// </summary>
        private struct ScheduledTickable
        {
            public ITickable Tickable;
            public int TicksPerRun;
            public int Countdown;
        }
    }
}
