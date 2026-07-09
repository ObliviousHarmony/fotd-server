using System.Threading.Channels;

namespace FOMServer.Shared.Infrastructure.Logging
{
    internal class BackgroundLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ChannelWriter<LogMessage> _writer;

        public BackgroundLogger(string categoryName, ChannelWriter<LogMessage> writer)
        {
            _categoryName = categoryName;
            _writer = writer;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = new LogMessage
            {
                Category = _categoryName,
                Level = logLevel,
                Message = formatter(state, exception),
                Exception = exception,
                Timestamp = DateTime.UtcNow
            };

            _writer.TryWrite(message);
        }
    }
}
