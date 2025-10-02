using FOMServer.Shared.Core;

namespace FOMServer.Application.Core
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly object syncRoot = new();
        private readonly CancellationTokenSource rootCts = new();
        private readonly List<Task> trackedTasks = new();
        private readonly TaskCompletionSource stoppingTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource stoppedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Stopping => stoppingTcs.Task;
        public Task Stopped => stoppedTcs.Task;

        public CancellationToken Token => rootCts.Token;

        public void TrackTask(Task task)
        {
            if (rootCts.IsCancellationRequested)
                throw new InvalidOperationException("Cannot track tasks after shutdown has been initiated.");

            lock (syncRoot)
                trackedTasks.Add(task);
        }

        public async Task Shutdown()
        {
            if (rootCts.IsCancellationRequested)
                return;

            rootCts.Cancel();
            stoppingTcs.TrySetResult();

            Task[] tasksToWait;
            lock (syncRoot)
                tasksToWait = trackedTasks.ToArray();
            await Task.WhenAll(tasksToWait);
            stoppedTcs.TrySetResult();
        }
    }

}
