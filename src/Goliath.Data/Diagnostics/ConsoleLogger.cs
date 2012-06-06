﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Diagnostics
{
    public class ConsoleLogger : Logger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        public ConsoleLogger() : this(LogLevel.All) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        public ConsoleLogger(LogLevel logLevel)
            : base(logLevel)
        {
        }

        #region ILogger Members

        /// <summary>
        /// Logs the specified log type.
        /// </summary>
        /// <param name="LogLevel">Type of the log.</param>
        /// <param name="message">The message.</param>
        public override void Log(string sessionId, LogLevel LogLevel, string message)
        {          
            if (CanLog(LogLevel))
            {
                string sessionText = string.Empty;
                if (!string.IsNullOrEmpty(sessionId))
                    sessionText = string.Format("Session {0} -", sessionId);

                var currentColor = Console.ForegroundColor;
                Console.Write("\n{0} {1}:{2}", sessionText, LogLevel.ToString(), DateTime.Now.ToString());

                switch (LogLevel)
                {
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case LogLevel.Fatal:
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    default:
                        break;
                    
                }                
                Console.Write(" - {0}\n", message);
                Console.ForegroundColor = currentColor;
            }
        }

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public override void LogException(string sessionId, string message, Exception exception)
        {
            Log(sessionId, LogLevel.Error, string.Format("{0}\n{1}", message, exception.ToString()));
        }

        #endregion
    }
}
