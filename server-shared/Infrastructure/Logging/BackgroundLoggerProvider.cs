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

        public BackgroundLoggerProvider(IShutdownManager shutdownManager, bool writeConsole = true, string? logFilePath = null)
        {
            _shutdownManager = shutdownManager;

            _channel = Channel.CreateUnbounded<LogMessage>(
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            );

            _writeConsole = writeConsole;

            if (logFilePath != null)
            {
                _logFileWriter = new StreamWriter(File.Open(
                   logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true
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

            if (_processingTask != null)
                await _processingTask;

            _logFileWriter?.Dispose();
        }

        public void Start()
        {
            if (_processingTask != null)
                return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownManager.Token);

            _processingTask = Task.Run(() => ProcessLoopAsync(_cts.Token), _cts.Token);

            _shutdownManager.TrackTask(_processingTask);
        }

        private async Task ProcessLoopAsync(CancellationToken ct)
        {
            try
            {
                await foreach (var message in _channel.Reader.ReadAllAsync(ct))
                    await WriteMessage(message);
            }
            catch (OperationCanceledException)
            {
            }

            // Drain remaining messages after cancellation
            while (_channel.Reader.TryRead(out var message))
                await WriteMessage(message);
        }

        private async Task WriteMessage(LogMessage message)
        {
            var formatted = message.Format();

            if (_writeConsole)
                Console.WriteLine(formatted);

            if (_logFileWriter != null)
                await _logFileWriter.WriteLineAsync(formatted);
        }
    }
}
