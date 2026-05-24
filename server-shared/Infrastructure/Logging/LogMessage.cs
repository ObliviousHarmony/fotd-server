namespace FOMServer.Shared.Infrastructure.Logging
{
    internal readonly struct LogMessage
    {
        public required string Category { get; init; }
        public required LogLevel Level { get; init; }
        public required string Message { get; init; }
        public required Exception? Exception { get; init; }
        public required DateTime Timestamp { get; init; }

        public string Format()
        {
            var levelStr = Level switch
            {
                LogLevel.Trace => "Trace",
                LogLevel.Debug => "Debug",
                LogLevel.Information => "Info",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                _ => "Info"
            };

            var prefix = $"[{Timestamp:O}][{levelStr}]: ";

            if (Exception is not null)
                return $"{prefix}{FormatException(Message, Exception)}";

            return $"{prefix}{Message}";
        }

        private static string FormatException(string message, Exception ex)
        {
            if (string.IsNullOrWhiteSpace(message))
                return $"[{ex.GetType().Name}]: {ex.Message}";

            return $"{message}\n  [{ex.GetType().Name}]: {ex.Message}";
        }
    }
}
