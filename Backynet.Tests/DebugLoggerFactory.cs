using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Backynet.Tests;

public class DebugLoggerFactory : ILoggerFactory
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DebugLoggerFactory(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new DebugLogger(_testOutputHelper);
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public class DebugLogger : ILogger, IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DebugLogger(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _testOutputHelper.WriteLine(formatter(state, exception));

            if (exception != null)
            {
                _testOutputHelper.WriteLine(exception.ToString());
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}