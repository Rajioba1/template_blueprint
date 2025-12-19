using Microsoft.Extensions.Logging;

namespace TemplateBlueprint.Core.Contracts;

/// <summary>
/// First-class logging console sink.
/// Routes ILogger calls to a visual console window.
/// </summary>
public interface ILogConsoleSink
{
    /// <summary>
    /// Logs a message at the specified level.
    /// </summary>
    void Log(LogLevel level, string message);

    /// <summary>
    /// Logs an exception with context.
    /// </summary>
    void LogException(Exception ex, string context);

    /// <summary>
    /// Shows the console window.
    /// </summary>
    void Show();

    /// <summary>
    /// Hides the console window.
    /// </summary>
    void Hide();

    /// <summary>
    /// Clears all console output.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets whether the console window is currently visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets all log entries (optionally redacted).
    /// </summary>
    /// <param name="redacted">If true, sensitive data is redacted.</param>
    /// <returns>The log entries as a string.</returns>
    string GetLogs(bool redacted = true);
}

