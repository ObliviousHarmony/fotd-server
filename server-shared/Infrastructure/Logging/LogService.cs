using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.Logging;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace FOMServer.Shared.Infrastructure.Logging
{
    public class LogService : ILogService
    {
        private static readonly Meter Meter = new("FOMServer.Logging");
        private static readonly Counter<long> LogEnqueuedCounter =
            Meter.CreateCounter<long>("log.entries.enqueued", unit: "entries", description: "Number of log entries enqueued");

        private readonly IShutdownManager shutdownManager;
        private readonly Channel<LogEntry> logChannel;
        private readonly bool writeConsole;
        private readonly StreamWriter? logFileWriter;

        private Task? loggingTask;
        private CancellationTokenSource? cts;

        public LogService(IShutdownManager shutdownManager, bool writeConsole = true, string? logFilePath = null)
        {
            this.shutdownManager = shutdownManager;

            logChannel = Channel.CreateUnbounded<LogEntry>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                }
            );

            this.writeConsole = writeConsole;

            if (logFilePath != null)
            {
                logFileWriter = new StreamWriter(File.Open(
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
            if (!logChannel.Writer.TryWrite(entry))
                throw new InvalidOperationException("Logging channel is closed.");

            LogEnqueuedCounter.Add(1, KeyValuePair.Create<string, object?>("level", entry.Level.ToString()));
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
        public void WritePacketException(Packet packet, Exception ex)
        {
            Write(PacketExceptionLogEntry.Create(packet, ex));
        }

        /// <summary>
        /// Starts the background logging task.
        /// </summary>
        public void Start()
        {
            if (loggingTask != null)
                return;

            cts = CancellationTokenSource.CreateLinkedTokenSource(shutdownManager.Token);

            loggingTask = Task.Run(() => ProcessLoopAsync(cts.Token), cts.Token);

            // Make sure that the shutdown manager waits for this task to complete.
            shutdownManager.TrackTask(loggingTask);
        }

        /// <summary>
        /// Main loop that consumes log entries from the channel.
        /// </summary>
        private async Task ProcessLoopAsync(CancellationToken ct)
        {
            try
            {
                await foreach (var entry in logChannel.Reader.ReadAllAsync(ct))
                {
                    var formatted = entry.ToString();

                    if (writeConsole)
                        Console.WriteLine(formatted);

                    if (logFileWriter != null)
                        await logFileWriter.WriteLineAsync(formatted);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                logFileWriter?.Dispose();
            }
        }
    }
}
