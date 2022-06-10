using System;
using Microsoft.Extensions.Logging;
using Utility;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NGInstaller.Core.CrossCutting
{
    /// <summary>
    /// separate by minutes
    /// </summary>
    public class LocalTextFileLogger : ILogger
    {
        private readonly TimeSeparationTextWriter _textWriter;

        public string CategoryName { get; }

        public LocalTextFileLogger(string categoryName, TimeSeparationTextWriter textWriter)
        {
            this.CategoryName = categoryName;
            _textWriter = textWriter;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var formatLog = LogExtensions.FormatLog(CategoryName, logLevel, eventId, state, exception, formatter);
            _textWriter.Append(formatLog);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}