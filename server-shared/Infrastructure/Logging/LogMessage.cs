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

            return Exception is not null ? $"{prefix}{FormatException(Message, Exception)}" : $"{prefix}{Message}";
        }

        private static string FormatException(string message, Exception ex)
        {
            return string.IsNullOrWhiteSpace(message)
                ? $"[{ex.GetType().Name}]: {ex.Message}"
                : $"{message}\n  [{ex.GetType().Name}]: {ex.Message}";
        }
    }
}
