using System;
using System.Globalization;
using Spectre.Console;

namespace Goliath.Data.CodeGenerator
{
    public class ConsoleLogger : Logger
    {
        private string sourceContext;
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        public ConsoleLogger(string sourceContext) : this(LogLevel.Debug, sourceContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="sourceContext"></param>
        public ConsoleLogger(LogLevel logLevel, string sourceContext)
            : base(logLevel)
        {
            if (string.IsNullOrWhiteSpace(sourceContext))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(sourceContext));

            this.sourceContext = sourceContext.Replace("Goliath.Data.", string.Empty);
        }

        #region ILogger Members

        /// <inheritdoc/>
        public override void Log(LogLevel logLevel, string message, long? sessionId = null)
        {
            LogInternal(logLevel, message, null, sessionId);
        }

        /// <inheritdoc/>
        public override void Log(LogLevel logLevel, string message, Exception exception, long? sessionId = null)
        {
            LogInternal(logLevel, message, exception, sessionId);
        }

        void LogInternal(LogLevel logLevel, string message, Exception exception, long? sessionId)
        {
            if (CanLog(logLevel))
            {
                string sessionText = string.Empty;
                if (sessionId.HasValue)
                    sessionText = $"Session {sessionId} -";

                AnsiConsole.MarkupLine($"[#005f00]{DateTime.Now:s}[/] {FormatLevel(logLevel)} [#005fd7]{sessionText} {Markup.Escape(sourceContext)}[/] {Markup.Escape(message)}");

                if (exception != null)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.WriteException(exception);
                    AnsiConsole.WriteLine();
                }
            }
        }

        string FormatLevel(LogLevel logLevel)
        {
            string format;
            switch (logLevel)
            {
                case LogLevel.Debug:
                    format = $"[#005fd7][[{logLevel.ToString()}]][/]";
                    break;
                case LogLevel.Fatal:
                case LogLevel.Error:
                    format = $"[#ff0000][[{logLevel.ToString()}]][/]";
                    break;
                case LogLevel.Warning:
                    format = $"[#ff5f00][[{logLevel.ToString()}]][/]";
                    break;
                case LogLevel.Info:
                    format = $"[#808000][[{logLevel.ToString()}]][/]";
                    break;
                default:
                    format = $"[#f2ffe6][[{logLevel.ToString()}]][/]";
                    break;

            }

            return format;
        }

        #endregion
    }
}