using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Shard.Shared.Web.IntegrationTests;

public class XunitLogger : ILogger
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly string categoryName;

    public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName)
    {
        this.testOutputHelper = testOutputHelper;
        this.categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state)
        => NoopDisposable.Instance;

    public bool IsEnabled(LogLevel logLevel)
        => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        testOutputHelper.WriteLine($"{categoryName} [{eventId}] {formatter(state, exception)}");
        if (exception != null)
            testOutputHelper.WriteLine(exception.ToString());
    }

    private class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance = new();
        public void Dispose()
        { }
    }
}
