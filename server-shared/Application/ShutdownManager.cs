using FOMServer.Shared.Core;

namespace FOMServer.Application.Core
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly object _syncRoot;
        private readonly CancellationTokenSource _rootCts;
        private readonly List<Task> _trackedTasks;
        private readonly TaskCompletionSource _stoppingTcs;
        private readonly TaskCompletionSource _stoppedTcs;

        public ShutdownManager()
        {
            _syncRoot = new();
            _rootCts = new CancellationTokenSource();
            _trackedTasks = new List<Task>();
            _stoppingTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _stoppedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public Task Stopping => _stoppingTcs.Task;
        public Task Stopped => _stoppedTcs.Task;

        public CancellationToken Token => _rootCts.Token;

        public void TrackTask(Task task)
        {
            if (_rootCts.IsCancellationRequested)
                throw new InvalidOperationException("Cannot track tasks after shutdown has been initiated");

            lock (_syncRoot)
                _trackedTasks.Add(task);
        }

        public void StartShutdown()
        {
            _ = Shutdown();
        }

        public async Task Shutdown()
        {
            if (_rootCts.IsCancellationRequested)
                return;

            _rootCts.Cancel();
            _stoppingTcs.TrySetResult();

            Task[] tasksToWait;
            lock (_syncRoot)
                tasksToWait = _trackedTasks.ToArray();
            await Task.WhenAll(tasksToWait);
            _stoppedTcs.TrySetResult();
        }
    }
}
