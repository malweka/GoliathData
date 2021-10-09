using System;

namespace Goliath.Data
{
    public static class LoggerExtensions
    {
        public static void Debug(this ILogger logger, string message, long? sessionId = null)
        {
            logger.Log(LogLevel.Debug, message, sessionId);
        }

        public static void Debug(this ILogger logger, string message, Exception exception, long? sessionId = null)
        {
            logger.Log(LogLevel.Debug, message, exception, sessionId);
        }

        public static void Info(this ILogger logger, string message, long? sessionId = null)
        {
            logger.Log(LogLevel.Info, message, sessionId);
        }

        public static void Info(this ILogger logger, string message, Exception exception, long? sessionId = null)
        {
            logger.Log(LogLevel.Info, message, exception, sessionId);
        }

        public static void Warning(this ILogger logger, string message, long? sessionId = null)
        {
            logger.Log(LogLevel.Warning, message, sessionId);
        }

        public static void Warning(this ILogger logger, string message, Exception exception, long? sessionId = null)
        {
            logger.Log(LogLevel.Warning, message, exception, sessionId);
        }

        public static void Error(this ILogger logger, string message, long? sessionId = null)
        {
            logger.Log(LogLevel.Error, message, sessionId);
        }

        public static void Error(this ILogger logger, string message, Exception exception, long? sessionId = null)
        {
            logger.Log(LogLevel.Error, message, exception, sessionId);
        }

        public static void Error(this ILogger logger, Exception exception, long? sessionId = null)
        {
            logger.Log(LogLevel.Error, exception?.Message, exception, sessionId);
        }
    }
}