using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Diagnostics
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Logger : ILogger //TODO: log context like view data or whatever and also to track group them together such as a group by Id and also
    {
        LogType currentLogType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        protected Logger(LogLevel logLevel)
        {
            CurrentLevel = logLevel;
            int levelInt = (int)logLevel;
            SetLogType(levelInt);
        }

        void SetLogType(int levelInt)
        {
            if (levelInt < 8)
            {
                try
                {
                    currentLogType = (LogType)Enum.ToObject(typeof(LogType), levelInt);
                }
                catch
                {
                    currentLogType = LogType.Fatal;
                }
            }
            else
                currentLogType = LogType.Fatal;
        }

        /// <summary>
        /// Determines whether this instance can log the specified log type.
        /// </summary>
        /// <param name="logType">Type of the log.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can log the specified log type; otherwise, <c>false</c>.
        /// </returns>
        protected bool CanLog(LogType logType)
        {
            if (logType >= currentLogType)
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
        /// <param name="logType">Type of the log.</param>
        /// <param name="message">The message.</param>
        public abstract void Log(string sessionId, LogType logType, string message);

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public abstract void LogException(string sessionId, string message, Exception exception);

        /// <summary>
        /// Logs the specified log type.
        /// </summary>
        /// <param name="logType">Type of the log.</param>
        /// <param name="message">The message.</param>
        public virtual void Log(LogType logType, string message)
        {
            Log(null, logType, message);
        }

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
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
