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

		private Thread? loggingThread;
		private readonly Channel<LogEntry> logChannel;
		private readonly bool writeConsole;
		private StreamWriter? logFileWriter;

		public LogService(bool writeConsole = true, string? logFilePath = null)
		{
			this.logChannel = Channel.CreateUnbounded<LogEntry>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = false
			});

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
		/// Disposes of the service.
		/// </summary>
		public void Dispose()
		{
			Stop();
			if (this.logFileWriter != null)
			{
				this.logFileWriter.Dispose();
				this.logFileWriter = null;
			}
		}

		/// <summary>
		/// Enqueue a log entry for processing.
		/// </summary>
		public void Enqueue(LogEntry entry)
		{
			if (!logChannel.Writer.TryWrite(entry))
				throw new InvalidOperationException("Logging channel is closed.");

			LogEnqueuedCounter.Add(1, KeyValuePair.Create<string, object?>("level", entry.Level.ToString()));
		}

		/// <summary>
		/// Starts the dedicated logging thread.
		/// </summary>
		public void Run()
		{
			if (this.loggingThread != null)
				return;

			loggingThread = new Thread(() =>
			{
				ProcessLoopAsync().GetAwaiter().GetResult();
			})
			{
				IsBackground = true,
				Name = "LoggingServiceThread"
			};
			loggingThread.Start();
		}

		/// <summary>
		/// Main loop that consumes log entries from the channel.
		/// </summary>
		private async Task ProcessLoopAsync()
		{
			await foreach (var entry in this.logChannel.Reader.ReadAllAsync())
			{
				var formatted = entry.ToString();

				if (this.writeConsole)
					Console.WriteLine(formatted);
				if (this.logFileWriter != null)
					await this.logFileWriter.WriteLineAsync(formatted);
			}
		}

		/// <summary>
		/// Stop the logging service gracefully.
		/// </summary>
		public void Stop()
		{
			if (this.loggingThread == null)
				return;

			this.logChannel.Writer.Complete();
			this.loggingThread.Join();
			this.loggingThread = null;
		}
	}
}
