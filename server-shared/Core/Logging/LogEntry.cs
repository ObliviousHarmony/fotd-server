using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.Shared.Core.Logging
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
            Message,
            Exception,
            PacketException
        }

        public EntryType Type;
        public LogLevel Level;
        public DateTime Timestamp;

        // Variant Data
        public MessageLogEntry Message;
        public ExceptionLogEntry Exception;
        public PacketExceptionLogEntry PacketException;

        /// <summary>
        /// Formats the log entry as a string for output.
        /// </summary>
        public override readonly string ToString()
        {
            return Type switch
            {
                EntryType.Message => Message.Format(Level, Timestamp),
                EntryType.Exception => Exception.Format(Timestamp),
                EntryType.PacketException => PacketException.Format(Timestamp),
                _ => $"[{Timestamp:O}][{Level}] <unknown entry>"
            };
        }
    }

    /// <summary>
    /// Represents a log entry with a message.
    /// </summary>
    public struct MessageLogEntry
    {
        public string Text { get; private set; }

        public string Format(LogLevel level, DateTime timestamp)
        {
            return $"[{timestamp:O}][{level}]: {Text}";
        }

        public static LogEntry Create(LogLevel level, string text)
        {
            return new LogEntry
            {
                Type = LogEntry.EntryType.Message,
                Level = level,
                Timestamp = DateTime.UtcNow,
                Message = new MessageLogEntry
                {
                    Text = text
                }
            };
        }
    }

    public struct ExceptionLogEntry
    {
        public Exception Exception { get; private set; }
        public string Format(DateTime timestamp)
        {
            return $"[{timestamp:O}][{LogLevel.Critical}]: {Exception}";
        }
        public static LogEntry Create(Exception ex)
        {
            return new LogEntry
            {
                Type = LogEntry.EntryType.Exception,
                Level = LogLevel.Critical,
                Timestamp = DateTime.UtcNow,
                Exception = new ExceptionLogEntry
                {
                    Exception = ex
                }
            };
        }
    }

    public struct PacketExceptionLogEntry
    {
        public Packet Packet { get; private set; }
        public Exception Exception { get; private set; }

        public string Format(DateTime timestamp)
        {
            return $"[{timestamp:O}][{LogLevel.Critical}]: Packet {Packet.ID} from {Packet.Sender}: {Exception}";
        }

        public static LogEntry Create(Packet packet, Exception ex)
        {
            return new LogEntry
            {
                Type = LogEntry.EntryType.PacketException,
                Level = LogLevel.Critical,
                Timestamp = DateTime.UtcNow,
                PacketException = new PacketExceptionLogEntry
                {
                    Packet = packet,
                    Exception = ex
                }
            };
        }
    }
}
