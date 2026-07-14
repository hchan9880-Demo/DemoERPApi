using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace DemoERPApi.Tests.Fixtures
{
    /// <summary>
    /// Test logger provider that stores all log entries in memory.
    /// </summary>
    public class TestLoggerProvider : ILoggerProvider
    {
        public ConcurrentBag<LogEntry> Logs { get; } = new();

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(categoryName, Logs);
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }

        public class LogEntry
        {
            public LogLevel LogLevel { get; set; }

            public string Category { get; set; } = string.Empty;

            public string Message { get; set; } = string.Empty;

            public Exception? Exception { get; set; }
        }

        private class TestLogger : ILogger
        {
            private readonly string _category;
            private readonly ConcurrentBag<LogEntry> _logs;

            public TestLogger(
                string category,
                ConcurrentBag<LogEntry> logs)
            {
                _category = category;
                _logs = logs;
            }

            public IDisposable BeginScope<TState>(TState state)
                where TState : notnull
            {
                return NullScope.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (formatter == null)
                    return;

                _logs.Add(new LogEntry
                {
                    LogLevel = logLevel,
                    Category = _category,
                    Message = formatter(state, exception),
                    Exception = exception
                });
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}