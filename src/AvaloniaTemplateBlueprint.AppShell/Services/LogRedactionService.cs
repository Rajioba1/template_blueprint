using System.Text.RegularExpressions;

namespace AvaloniaTemplateBlueprint.AppShell.Services;

/// <summary>
/// Service to redact sensitive information from log messages.
/// </summary>
public class LogRedactionService
{
    private readonly List<Regex> _secretPatterns = new();

    /// <summary>
    /// Gets the default secret patterns to redact.
    /// </summary>
    public static IReadOnlyList<string> DefaultPatterns => new[]
    {
        @"password[=:]\s*\S+",
        @"api[_-]?key[=:]\s*\S+",
        @"secret[=:]\s*\S+",
        @"token[=:]\s*\S+",
        @"bearer\s+\S+",
        @"authorization[=:]\s*\S+",
        @"connectionstring[=:]\s*\S+",
        @"pwd[=:]\s*\S+",
        @"credential[=:]\s*\S+"
    };

    /// <summary>
    /// Gets or sets the replacement text for redacted content.
    /// </summary>
    public string RedactionText { get; set; } = "[REDACTED]";

    /// <summary>
    /// Creates a new log redaction service with default patterns.
    /// </summary>
    public LogRedactionService()
    {
        foreach (var pattern in DefaultPatterns)
        {
            AddPattern(pattern);
        }
    }

    /// <summary>
    /// Adds a custom redaction pattern.
    /// </summary>
    /// <param name="pattern">Regular expression pattern to match sensitive data.</param>
    public void AddPattern(string pattern)
    {
        _secretPatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
    }

    /// <summary>
    /// Clears all redaction patterns.
    /// </summary>
    public void ClearPatterns()
    {
        _secretPatterns.Clear();
    }

    /// <summary>
    /// Redacts sensitive information from a message.
    /// </summary>
    /// <param name="message">The message to redact.</param>
    /// <returns>The redacted message.</returns>
    public string Redact(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var result = message;
        foreach (var pattern in _secretPatterns)
        {
            result = pattern.Replace(result, RedactionText);
        }

        return result;
    }

    /// <summary>
    /// Checks if a message contains potentially sensitive information.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns>True if the message may contain sensitive data.</returns>
    public bool ContainsSensitiveData(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        return _secretPatterns.Any(p => p.IsMatch(message));
    }
}
