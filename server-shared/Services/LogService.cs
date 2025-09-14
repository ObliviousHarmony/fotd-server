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
		private StreamWriter? logFileWriter;

		private Task? loggingTask;
		private CancellationTokenSource? cts;

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
		/// Enqueue a log entry for processing.
		/// </summary>
		public void Write(LogEntry entry)
		{
			if (!logChannel.Writer.TryWrite(entry))
				throw new InvalidOperationException("Logging channel is closed.");

			LogEnqueuedCounter.Add(1, KeyValuePair.Create<string, object?>("level", entry.Level.ToString()));
		}


		/// <summary>
		/// Starts the background logging task.
		/// </summary>
		/// <param name="parentToken">The parent's cancellation token.</param>
		public Task StartAsync(CancellationToken parentToken)
		{
			if (loggingTask != null)
				return Task.CompletedTask;

			cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

			loggingTask = Task.Factory.StartNew(
				async () => await ProcessLoopAsync(cts.Token),
				cts.Token,
				TaskCreationOptions.LongRunning,
				TaskScheduler.Default
			).Unwrap();

			return Task.CompletedTask;
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
				// Expected when cancellation occurs
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
			await foreach (var entry in this.logChannel.Reader.ReadAllAsync(ct))
			{
				var formatted = entry.ToString();

				if (this.writeConsole)
					Console.WriteLine(formatted);

				if (this.logFileWriter != null)
					await this.logFileWriter.WriteLineAsync(formatted);
			}
		}

		/// <summary>
		/// Dispose of resources and stop the service if needed.
		/// </summary>
		public void Dispose()
		{
			if (loggingTask != null)
				StopAsync().GetAwaiter().GetResult();

			if (logFileWriter != null)
				logFileWriter.Dispose();
		}
	}
}
