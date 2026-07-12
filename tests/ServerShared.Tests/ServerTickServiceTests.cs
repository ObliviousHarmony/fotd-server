using FOMServer.Shared.Application.Ticks;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Ticks;

namespace FOMServer.Shared.Tests
{
    public class ServerTickServiceTests : IDisposable
    {
        private readonly Mock<IShutdownManager> _shutdownManager;
        private readonly Mock<ILogger<ServerTickService>> _logger;
        private readonly CancellationTokenSource _cts;

        public ServerTickServiceTests()
        {
            _cts = new CancellationTokenSource();
            _shutdownManager = new Mock<IShutdownManager>();
            _shutdownManager.Setup(s => s.Token).Returns(_cts.Token);
            _shutdownManager.Setup(s => s.TrackTask(It.IsAny<Task>()));

            _logger = new Mock<ILogger<ServerTickService>>();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void TickOnce_RunsEachTickableOncePerInterval()
        {
            // Base period is 1 ms (the fastest tickable), so the 3 ms tickable runs every 3 steps.
            var fast = new FakeTickable(TimeSpan.FromMilliseconds(1));
            var slow = new FakeTickable(TimeSpan.FromMilliseconds(3));
            var service = CreateService(fast, slow);

            var due = new List<ITickable>();
            var fastRuns = 0;
            var slowRuns = 0;
            for (var step = 0; step < 9; step++)
            {
                service.TickOnce(due);
                if (due.Contains(fast))
                {
                    fastRuns++;
                }

                if (due.Contains(slow))
                {
                    slowRuns++;
                }
            }

            Assert.Equal(9, fastRuns);
            Assert.Equal(3, slowRuns);
        }

        [Fact]
        public void TickOnce_StaggersEqualIntervalTickables()
        {
            // A 1 ms base period makes both 2 ms tickables ticksPerRun = 2.
            var basePace = new FakeTickable(TimeSpan.FromMilliseconds(1));
            var a = new FakeTickable(TimeSpan.FromMilliseconds(2));
            var b = new FakeTickable(TimeSpan.FromMilliseconds(2));
            var service = CreateService(basePace, a, b);

            var due = new List<ITickable>();
            var aRuns = 0;
            var bRuns = 0;
            for (var step = 0; step < 6; step++)
            {
                service.TickOnce(due);

                Assert.False(
                    due.Contains(a) && due.Contains(b),
                    "equal-interval tickables must not fire on the same wake-up"
                );

                if (due.Contains(a))
                {
                    aRuns++;
                }

                if (due.Contains(b))
                {
                    bRuns++;
                }
            }

            // Both still fire on their own cadence, just offset from each other.
            Assert.Equal(3, aRuns);
            Assert.Equal(3, bRuns);
        }

        [Fact]
        public void TickOnce_MixedIntervals_EachFiresOnOwnCadence()
        {
            var every1 = new FakeTickable(TimeSpan.FromMilliseconds(1));
            var every2 = new FakeTickable(TimeSpan.FromMilliseconds(2));
            var every5 = new FakeTickable(TimeSpan.FromMilliseconds(5));
            var service = CreateService(every1, every2, every5);

            var due = new List<ITickable>();
            var runs1 = 0;
            var runs2 = 0;
            var runs5 = 0;
            for (var step = 0; step < 10; step++)
            {
                service.TickOnce(due);
                if (due.Contains(every1))
                {
                    runs1++;
                }

                if (due.Contains(every2))
                {
                    runs2++;
                }

                if (due.Contains(every5))
                {
                    runs5++;
                }
            }

            Assert.Equal(10, runs1);
            Assert.Equal(5, runs2);
            Assert.Equal(2, runs5);
        }

        private ServerTickService CreateService(params ITickable[] tickables)
        {
            return new ServerTickService(tickables, _shutdownManager.Object, _logger.Object);
        }

        private sealed class FakeTickable : ITickable
        {
            public int TickCount;

            public FakeTickable(TimeSpan interval)
            {
                TickInterval = interval;
            }

            public TimeSpan TickInterval { get; }

            public Func<CancellationToken, ValueTask>? OnTick { get; set; }

            public ValueTask TickAsync(CancellationToken cancellationToken)
            {
                TickCount++;
                return OnTick?.Invoke(cancellationToken) ?? ValueTask.CompletedTask;
            }
        }
    }
}
