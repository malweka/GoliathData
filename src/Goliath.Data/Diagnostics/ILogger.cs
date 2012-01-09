using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Diagnostics
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets the current level.
        /// </summary>
        /// <value>The current level.</value>
        LogLevel CurrentLevel { get; }

        /// <summary>
        /// Logs the specified log type.
        /// </summary>
        /// <param name="logType">Type of the log.</param>
        /// <param name="message">The message.</param>
        void Log(LogType logType, string message);

        /// <summary>
        /// Logs the specified log type.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="message">The message.</param>
        /// <param name="logType">Type of the log.</param>
        /// <param name="message">The message.</param>
        void Log(string sessionId,  LogType logType, string message);

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        void LogException(string sessionId, string message, Exception exception);

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void LogException(string message, Exception exception);
    }

    /// <summary>
    /// 
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 
        /// </summary>
        Info = 1,
        /// <summary>
        /// 
        /// </summary>
        Debug = 2,
        /// <summary>
        /// 
        /// </summary>
        Warning = 4,
        /// <summary>
        /// 
        /// </summary>
        Error = 8,
        /// <summary>
        /// 
        /// </summary>
        Fatal = 64,
        /// <summary>
        /// 
        /// </summary>
        All = Info
    }

    /// <summary>
    /// 
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 
        /// </summary>
        Info = 1,
        /// <summary>
        /// 
        /// </summary>
        Debug = 2,
        /// <summary>
        /// 
        /// </summary>
        Warning = 4,
        /// <summary>
        /// 
        /// </summary>
        Error = 8,
        /// <summary>
        /// 
        /// </summary>
        Fatal = 64,
    }
}
