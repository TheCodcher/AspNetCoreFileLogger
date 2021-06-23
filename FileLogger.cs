using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCUDWebIntegration.Logger
{
    public class FileLogger : ILogger
    {
        private int _baseLogLevel;
        private string category;
        private Action<string> writeText;
        public FileLogger(LogLevel minLvl, string category, Action<string> writeText)
        {
            if (writeText is null)
            {
                throw new ArgumentNullException(nameof(writeText));
            }
            _baseLogLevel = (int)minLvl;
            this.category = category;
            this.writeText = writeText;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None) return false;
            return (int)logLevel >= _baseLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var a = GetText(logLevel, eventId, state, exception, formatter);
            writeText(a);
        }

        private string GetText<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string stateString;
            if (exception != null)
            {
                if (formatter == null)
                {
                    stateString = ExceptionFormat(exception);
                }
                else
                {
                    stateString = formatter(state, exception);
                }
            }
            else
            {
                stateString = state.ToString();
            }
            var id = string.IsNullOrEmpty(eventId.Name) ? eventId.Id.ToString() : $"{eventId.Id} - {eventId.Name}";
            return $"[{DateTime.UtcNow}] {logLevel}: {category}({id}){Environment.NewLine}{stateString}{Environment.NewLine}";
        }

        private string ExceptionFormat(Exception exception)
        {
            return exception.Message;
        }
    }
}
