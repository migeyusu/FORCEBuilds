using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NGInstaller.Core.CrossCutting
{
    public class LocalTextFileLoggerProvider : ILoggerProvider
    {
        private readonly TimeSeparationTextWriter _textWriter;

        private readonly List<LocalTextFileLogger> _loggers = new List<LocalTextFileLogger>();

        public LocalTextFileLoggerProvider(IOptions<TextLoggerProviderOptions> options)
        {
            var loggerProviderOptions = options.Value;
            var logPath = loggerProviderOptions.Root;
            if (string.IsNullOrEmpty(logPath))
            {
                logPath = ".";
            }

            var rootPath = Toolkit.GetApplicationRoot();
            var logFolderPath = Path.GetFullPath(logPath, rootPath);
            _textWriter = new TimeSeparationTextWriter(logFolderPath);
        }

        public void Dispose()
        {
            _textWriter.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            var textLogger = _loggers.FirstOrDefault(logger => logger.CategoryName == categoryName);
            if (textLogger == null)
            {
                textLogger = new LocalTextFileLogger(categoryName, _textWriter);
                _loggers.Add(textLogger);
            }

            return textLogger;
        }
    }
}