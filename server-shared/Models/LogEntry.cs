using FOMServer.Shared.Enums;

namespace FOMServer.Shared.Models
{
	/// <summary>
	/// Represents a log entry to be enqueued.
	/// </summary>
	public struct LogEntry
	{
		/// <summary>
		/// The type of the log entry so that we
		/// know which formatting field to use.
		/// </summary>
		public enum EntryType : byte
		{
			Message
		}

		public EntryType Type;
		public LogLevel Level;
		public DateTime Timestamp;

		// Variant Data
		public MessageLogEntry Message;

		/// <summary>
		/// Formats the log entry as a string for output.
		/// </summary>
		public override string ToString()
		{
			return Type switch
			{
				EntryType.Message => Message.Format(Level, Timestamp),
				_ => $"[{Timestamp:O}][{Level}] <unknown entry>"
			};
		}
	}

	/// <summary>
	/// Represents a log entry with a message.
	/// </summary>
	public struct MessageLogEntry
	{
		public string Text;

		public string Format(LogLevel level, DateTime timestamp)
		{
			return $"[{timestamp:O}][{level}]: {Text}";
		}
	}
}
