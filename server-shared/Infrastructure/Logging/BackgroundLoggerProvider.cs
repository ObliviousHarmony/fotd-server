using System.Collections.Concurrent;
using System.Threading.Channels;
using FOMServer.Shared.Core;

namespace FOMServer.Shared.Infrastructure.Logging
{
    internal sealed class BackgroundLoggerProvider : ILoggerProvider, IAsyncDisposable
    {
        private readonly IShutdownManager _shutdownManager;
        private readonly Channel<LogMessage> _channel;
        private readonly ConcurrentDictionary<string, BackgroundLogger> _loggers = new();
        private readonly bool _writeConsole;
        private readonly StreamWriter? _logFileWriter;

        private Task? _processingTask;
        private CancellationTokenSource? _cts;

        public BackgroundLoggerProvider(
            IShutdownManager shutdownManager,
            bool writeConsole = true,
            string? logFilePath = null
        )
        {
            _shutdownManager = shutdownManager;

            _channel = Channel.CreateUnbounded<LogMessage>(new UnboundedChannelOptions { SingleReader = true });

            _writeConsole = writeConsole;

            if (logFilePath is not null)
            {
                _logFileWriter = new StreamWriter(
                    File.Open(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read)
                )
                {
                    AutoFlush = true,
                };
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new BackgroundLogger(name, _channel.Writer));
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            _channel.Writer.Complete();

            if (_processingTask is not null)
            {
                await _processingTask;
            }

            _cts?.Dispose();
            _logFileWriter?.Dispose();
        }

        public void Start()
        {
            if (_processingTask is not null)
            {
                return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownManager.Token);

            _processingTask = Task.Run(() => ProcessLoopAsync(_cts.Token), _cts.Token);

            _shutdownManager.TrackTask(_processingTask);
        }

        private async Task ProcessLoopAsync(CancellationToken ct)
        {
            try
            {
                await foreach (var message in _channel.Reader.ReadAllAsync(ct))
                {
                    await TryWriteMessage(message);
                }
            }
            catch (OperationCanceledException) { }

            // Drain remaining messages after cancellation
            while (_channel.Reader.TryRead(out var message))
            {
                await TryWriteMessage(message);
            }
        }

        private async Task TryWriteMessage(LogMessage message)
        {
            try
            {
                var formatted = message.Format();

                if (_writeConsole)
                {
                    var target = message.Level >= LogLevel.Warning ? Console.Error : Console.Out;
                    await target.WriteLineAsync(formatted);
                }

                if (_logFileWriter is not null)
                {
                    await _logFileWriter.WriteLineAsync(formatted);
                }
            }
            catch (Exception ex)
            {
                // Don't let exceptions silently crash the entire thread.
                try
                {
                    Console.Error.WriteLine($"[BackgroundLogger] Failed to write log message: {ex.Message}");
                }
                catch
                {
                    // There is absolutely nothing we can do here.
                }
            }
        }
    }
}
