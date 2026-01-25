namespace FOMServer.Shared.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogTrace(this ILogger logger, Exception ex)
            => logger.Log(LogLevel.Trace, ex, "");

        public static void LogDebug(this ILogger logger, Exception ex)
            => logger.Log(LogLevel.Debug, ex, "");

        public static void LogInformation(this ILogger logger, Exception ex)
            => logger.Log(LogLevel.Information, ex, "");

        public static void LogWarning(this ILogger logger, Exception ex)
            => logger.Log(LogLevel.Warning, ex, "");

        public static void LogError(this ILogger logger, Exception ex)
            => logger.Log(LogLevel.Error, ex, "");

        public static void LogCritical(this ILogger logger, Exception ex)
            => logger.Log(LogLevel.Critical, ex, "");
    }
}
