using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace FOMServer.Shared.Services
{
	public class LogService : ILogService, IDisposable
	{
		private static readonly Meter Meter = new("FOMServer.Logging");
		private static readonly Counter<long> LogEnqueuedCounter =
			Meter.CreateCounter<long>("log.entries.enqueued", unit: "entries", description: "Number of log entries enqueued");

		private readonly Channel<LogEntry> logChannel;
		private readonly bool writeConsole;
		private readonly StreamWriter? logFileWriter;

		private Task? loggingTask;
		private CancellationTokenSource? cts;

		public LogService(bool writeConsole = true, string? logFilePath = null)
		{
			this.logChannel = Channel.CreateUnbounded<LogEntry>(
				new UnboundedChannelOptions
				{
					SingleReader = true,
					SingleWriter = false
				}
			);

			this.writeConsole = writeConsole;

			if (logFilePath != null)
			{
				this.logFileWriter = new StreamWriter(File.Open(
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
	/// <param name="level">The log level to use for the message.</param>
	/// <param name="message">The message to write.</param>
		public void WriteMessage(LogLevel level, string message)
		{
			Write(MessageLogEntry.Create(level, message));
		}

		/// <summary>
		/// Starts the background logging task.
		/// </summary>
		/// <param name="ctParent">The parent's cancellation token.</param>
		public void Start(CancellationToken ctParent)
		{
			if (loggingTask != null)
				return;

			cts = CancellationTokenSource.CreateLinkedTokenSource(ctParent);

			loggingTask = Task.Factory.StartNew(
				async () => await ProcessLoopAsync(cts.Token),
				cts.Token,
				TaskCreationOptions.LongRunning,
				TaskScheduler.Default
			).Unwrap();
		}

		/// <summary>
		/// Stops the logging service gracefully.
		/// </summary>
		public async Task StopAsync()
		{
			if (loggingTask == null)
				return;

			cts?.Cancel();
			logChannel.Writer.Complete();

			try
			{
				await loggingTask;
			}
			catch (OperationCanceledException)
			{
			}

			loggingTask = null;
			cts?.Dispose();
			cts = null;
		}

		/// <summary>
		/// Main loop that consumes log entries from the channel.
		/// </summary>
		private async Task ProcessLoopAsync(CancellationToken ct)
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

		/// <summary>
		/// Dispose of resources and stop the service if needed.
		/// </summary>
		public void Dispose()
		{
			if (loggingTask != null)
				StopAsync().GetAwaiter().GetResult();

			logFileWriter?.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}
