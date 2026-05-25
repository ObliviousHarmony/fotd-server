using FOMServer.Shared.Core;

namespace FOMServer.Shared.Application
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly Lock _syncRoot = new();
        private readonly CancellationTokenSource _rootCts = new();
        private readonly List<Task> _trackedTasks = [];
        private readonly TaskCompletionSource _stoppingTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _stoppedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Stopping => _stoppingTcs.Task;

        public Task Stopped => _stoppedTcs.Task;

        public CancellationToken Token => _rootCts.Token;

        public void TrackTask(Task task)
        {
            if (_rootCts.IsCancellationRequested)
            {
                throw new InvalidOperationException("Cannot track tasks after shutdown has been initiated");
            }

            lock (_syncRoot)
            {
                _trackedTasks.Add(task);
            }
        }

        public void StartShutdown()
        {
            _ = Shutdown();
        }

        public async Task Shutdown()
        {
            if (_rootCts.IsCancellationRequested)
            {
                return;
            }

            _rootCts.Cancel();
            _ = _stoppingTcs.TrySetResult();

            Task[] tasksToWait;
            lock (_syncRoot)
            {
                tasksToWait = [.. _trackedTasks];
            }

            await Task.WhenAll(tasksToWait);
            _ = _stoppedTcs.TrySetResult();
        }
    }
}
