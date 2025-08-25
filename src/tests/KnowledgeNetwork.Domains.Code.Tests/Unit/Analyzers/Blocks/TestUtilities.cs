using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Blocks;

/// <summary>
/// Simple test logger that captures log messages for debugging
/// </summary>
public class TestLogger<T> : ILogger<T>
{
    public List<string> LogMessages { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = $"[{logLevel}] {formatter(state, exception)}";
        if (exception != null)
        {
            message += $" Exception: {exception}";
        }
        LogMessages.Add(message);
    }
}