using System;

namespace Goliath.Data.Diagnostics
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Logger : ILogger //TODO: log context like view data or whatever and also to track group them together such as a group by Id and also
    {
        LogLevel currentLogLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        protected Logger(LogLevel logLevel)
        {
            CurrentLevel = logLevel;
            currentLogLevel = logLevel;
            
        }

        /// <summary>
        /// Determines whether this instance can log the specified log type.
        /// </summary>
        /// <param name="LogLevel">Type of the log.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can log the specified log type; otherwise, <c>false</c>.
        /// </returns>
        protected bool CanLog(LogLevel LogLevel)
        {
            if (LogLevel >= currentLogLevel)
            {
                return true;
            }

            return false;
        }

        #region ILogger Members

        /// <summary>
        /// Gets or sets the current level.
        /// </summary>
        /// <value>The current level.</value>
        public LogLevel CurrentLevel { get; private set; }

        /// <summary>
        /// Logs the specified log type.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="LogLevel">Type of the log.</param>
        /// <param name="message">The message.</param>
        public abstract void Log(string sessionId, LogLevel LogLevel, string message);

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public abstract void LogException(string sessionId, string message, Exception exception);

        /// <summary>
        /// Logs the specified log type.
        /// </summary>
        /// <param name="LogLevel">Type of the log.</param>
        /// <param name="message">The message.</param>
        public virtual void Log(LogLevel LogLevel, string message)
        {
            Log(null, LogLevel, message);
        }

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public virtual void LogException(string message, Exception exception)
        {
            LogException(null, message, exception);
        }

        #endregion

        static Func<Type, ILogger> loggerFactoryMethod;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static ILogger GetLogger(Type type)
        {
            if (loggerFactoryMethod == null)
                return new ConsoleLogger();
            else
                return loggerFactoryMethod.Invoke(type);
        }

        internal static void SetLogger(Func<Type, ILogger> factoryMethod)
        {
            loggerFactoryMethod = factoryMethod;
        }
    }
}
