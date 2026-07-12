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
            if (Exception is not null)
            {
                return FormatException();
            }

            return FormatMessage();
        }

        private string FormatMessage()
        {
            return $"[{Timestamp:O}][{Level}]: {Message}";
        }

        private string FormatException()
        {
            // Indent the multi-line string.
            var exceptionText = Exception!.ToString().Replace(Environment.NewLine, $"{Environment.NewLine}    ");

            if (string.IsNullOrWhiteSpace(Message))
            {
                return $"[{Timestamp:O}][{Level}]: {exceptionText}";
            }

            return $"[{Timestamp:O}][{Level}]: {Message}{Environment.NewLine}    {exceptionText}";
        }
    }
}
