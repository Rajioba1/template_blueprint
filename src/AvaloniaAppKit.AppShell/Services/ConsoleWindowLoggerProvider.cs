using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AvaloniaAppKit.AppShell.Services;

/// <summary>
/// Log entry for display in the console window.
/// </summary>
public record LogEntry(
    DateTime Timestamp,
    LogLevel Level,
    string Category,
    string Message,
    Exception? Exception = null)
{
    /// <summary>
    /// Gets a formatted display string for the log entry.
    /// </summary>
    public string FormattedMessage =>
        Exception != null
            ? $"[{Timestamp:HH:mm:ss}] [{Level}] {Category}: {Message}\n{Exception}"
            : $"[{Timestamp:HH:mm:ss}] [{Level}] {Category}: {Message}";

    /// <summary>
    /// Gets the level display string.
    /// </summary>
    public string LevelDisplay => Level switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Critical => "CRT",
        _ => "???"
    };
}

/// <summary>
/// Event arguments for new log entries.
/// </summary>
public class LogEntryEventArgs : EventArgs
{
    public LogEntry Entry { get; }

    public LogEntryEventArgs(LogEntry entry)
    {
        Entry = entry;
    }
}

/// <summary>
/// Logger provider that routes logs to a visual console window.
/// </summary>
public class ConsoleWindowLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ConsoleWindowLogger> _loggers = new();
    private readonly LogRedactionService _redactionService;
    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new();

    /// <summary>
    /// Gets or sets the maximum number of log entries to keep.
    /// </summary>
    public int MaxEntries { get; set; } = 10000;

    /// <summary>
    /// Gets or sets whether to apply redaction to log messages.
    /// </summary>
    public bool EnableRedaction { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum log level to capture.
    /// </summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Raised when a new log entry is added.
    /// </summary>
    public event EventHandler<LogEntryEventArgs>? LogEntryAdded;

    /// <summary>
    /// Creates a new console window logger provider.
    /// </summary>
    /// <param name="redactionService">Optional redaction service.</param>
    public ConsoleWindowLoggerProvider(LogRedactionService? redactionService = null)
    {
        _redactionService = redactionService ?? new LogRedactionService();
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new ConsoleWindowLogger(name, this));
    }

    /// <summary>
    /// Adds a log entry to the console.
    /// </summary>
    internal void AddEntry(LogEntry entry)
    {
        if (entry.Level < MinLevel)
            return;

        var processedEntry = EnableRedaction
            ? entry with { Message = _redactionService.Redact(entry.Message) }
            : entry;

        lock (_lock)
        {
            _entries.Add(processedEntry);

            // Trim if exceeds max
            while (_entries.Count > MaxEntries)
            {
                _entries.RemoveAt(0);
            }
        }

        LogEntryAdded?.Invoke(this, new LogEntryEventArgs(processedEntry));
    }

    /// <summary>
    /// Gets all log entries.
    /// </summary>
    /// <param name="redacted">If true, applies redaction. If false, returns raw entries.</param>
    /// <returns>The log entries.</returns>
    public IReadOnlyList<LogEntry> GetEntries(bool redacted = true)
    {
        lock (_lock)
        {
            if (redacted || !EnableRedaction)
            {
                return _entries.ToList();
            }

            // Re-redact if requested
            return _entries.Select(e => e with { Message = _redactionService.Redact(e.Message) }).ToList();
        }
    }

    /// <summary>
    /// Gets log entries as a formatted string.
    /// </summary>
    /// <param name="redacted">If true, applies redaction.</param>
    /// <returns>Formatted log text.</returns>
    public string GetLogsAsText(bool redacted = true)
    {
        var entries = GetEntries(redacted);
        return string.Join(Environment.NewLine, entries.Select(e => e.FormattedMessage));
    }

    /// <summary>
    /// Clears all log entries.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _loggers.Clear();
    }
}

/// <summary>
/// Logger that sends entries to the console window.
/// </summary>
internal class ConsoleWindowLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ConsoleWindowLoggerProvider _provider;

    public ConsoleWindowLogger(string categoryName, ConsoleWindowLoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _provider.MinLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var entry = new LogEntry(DateTime.Now, logLevel, _categoryName, message, exception);
        _provider.AddEntry(entry);
    }
}
