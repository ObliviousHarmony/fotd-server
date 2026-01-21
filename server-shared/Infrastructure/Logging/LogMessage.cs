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

            if (Exception != null)
                return $"[{Timestamp:O}][{levelStr}]: {Message}\n{Exception}";

            return $"[{Timestamp:O}][{levelStr}]: {Message}";
        }
    }
}
