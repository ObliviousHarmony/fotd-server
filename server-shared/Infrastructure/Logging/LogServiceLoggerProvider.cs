using FOMServer.Shared.Core.Logging;
using Microsoft.Extensions.Logging;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;
using FomLogLevel = FOMServer.Shared.Core.Enums.LogLevel;

namespace FOMServer.Shared.Infrastructure.Logging
{
    public sealed class LogServiceLoggerProvider : ILoggerProvider
    {
        private readonly ILogService _logService;
        private readonly MsLogLevel _minimumLevel;

        public LogServiceLoggerProvider(ILogService logService, MsLogLevel minimumLevel = MsLogLevel.Warning)
        {
            _logService = logService;
            _minimumLevel = minimumLevel;
        }

        public ILogger CreateLogger(string categoryName) =>
            new LogServiceLogger(_logService, _minimumLevel);

        public void Dispose() { }
    }

    internal sealed class LogServiceLogger : ILogger
    {
        private readonly ILogService _logService;
        private readonly MsLogLevel _minimumLevel;

        public LogServiceLogger(ILogService logService, MsLogLevel minimumLevel)
        {
            _logService = logService;
            _minimumLevel = minimumLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(MsLogLevel logLevel) => logLevel >= _minimumLevel;

        public void Log<TState>(
            MsLogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            _logService.WriteMessage(MapLogLevel(logLevel), message);

            if (exception != null)
                _logService.WriteException(exception);
        }

        private static FomLogLevel MapLogLevel(MsLogLevel level) => level switch
        {
            MsLogLevel.Trace => FomLogLevel.Debug,
            MsLogLevel.Debug => FomLogLevel.Debug,
            MsLogLevel.Information => FomLogLevel.Info,
            MsLogLevel.Warning => FomLogLevel.Warning,
            MsLogLevel.Error => FomLogLevel.Error,
            MsLogLevel.Critical => FomLogLevel.Critical,
            _ => FomLogLevel.Info
        };
    }
}
