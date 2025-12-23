using System.Diagnostics.Metrics;
using System.Threading.Channels;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Logging;

namespace FOMServer.Shared.Infrastructure.Logging
{
    public class LogService : ILogService
    {
        private static readonly Meter s_meter = new("FOMServer.Logging");
        private static readonly Counter<long> s_logEnqueuedCounter =
            s_meter.CreateCounter<long>("log.entries.enqueued", unit: "entries", description: "Number of log entries enqueued");

        private readonly IShutdownManager _shutdownManager;
        private readonly Channel<LogEntry> _logChannel;
        private readonly bool _writeConsole;
        private readonly StreamWriter? _logFileWriter;

        private Task? _loggingTask;
        private CancellationTokenSource? _cts;

        public LogService(IShutdownManager shutdownManager, bool writeConsole = true, string? logFilePath = null)
        {
            _shutdownManager = shutdownManager;

            _logChannel = Channel.CreateUnbounded<LogEntry>(
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

        /// <summary>
        /// Enqueue a log entry for processing.
        /// </summary>
        public void Write(in LogEntry entry)
        {
            if (!_logChannel.Writer.TryWrite(entry))
                throw new InvalidOperationException("Logging channel is closed");

            s_logEnqueuedCounter.Add(1, KeyValuePair.Create<string, object?>("level", entry.Level.ToString()));
        }

        /// <summary>
        /// Write a log message with the specified level.
        /// </summary>
        /// <remarks>
        /// Please keep in mind that this method allocates a new string. This matters
        /// when doing things like string concatenation because the message might be
        /// discarded based on the log level. In general, prefer using custom
        /// LogEntry instances that defer message construction until needed.
        /// </remarks>
        public void WriteMessage(LogLevel level, string message)
        {
            Write(MessageLogEntry.Create(level, message));
        }

        /// <summary>
        /// Write a log entry for an exception.
        /// </summary>
        public void WriteException(Exception ex)
        {
            Write(ExceptionLogEntry.Create(ex));
        }

        /// <summary>
        /// Write a log entry for a packet exception.
        /// </summary>
        public void WritePacketException(in PacketRef packet, Exception ex)
        {
            Write(PacketExceptionLogEntry.Create(packet, ex));
        }

        /// <summary>
        /// Starts the background logging task.
        /// </summary>
        public void Start()
        {
            if (_loggingTask != null)
                return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownManager.Token);

            _loggingTask = Task.Run(() => ProcessLoopAsync(_cts.Token), _cts.Token);

            // Make sure that the shutdown manager waits for this task to complete.
            _shutdownManager.TrackTask(_loggingTask);
        }

        /// <summary>
        /// Main loop that consumes log entries from the channel.
        /// </summary>
        private async Task ProcessLoopAsync(CancellationToken ct)
        {
            try
            {
                await foreach (var entry in _logChannel.Reader.ReadAllAsync(ct))
                {
                    var formatted = entry.ToString();

                    if (_writeConsole)
                        Console.WriteLine(formatted);

                    if (_logFileWriter != null)
                        await _logFileWriter.WriteLineAsync(formatted);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _logFileWriter?.Dispose();
            }
        }
    }
}
