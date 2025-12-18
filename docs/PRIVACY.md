# Privacy & Log Handling

AvaloniaAppKit includes a debug console feature that captures application logs. This document explains the privacy implications and how sensitive data is handled.

## Debug Console Overview

The debug console (`DebugConsoleWindow`) captures log output from your application via `ConsoleWindowLoggerProvider`. This is useful for debugging but requires careful handling of sensitive data.

## What Gets Captured

### Default Behavior (ILoggerProvider)

The `ConsoleWindowLoggerProvider` captures:
- All messages sent through `ILogger<T>` interfaces
- Log levels: Debug, Information, Warning, Error, Critical
- Timestamps and log categories

### Opt-In: stdout/stderr Capture

The `StdOutCaptureService` is **opt-in only** and disabled by default. When enabled, it additionally captures:
- Console.WriteLine output
- Console.Error output
- Third-party library console output

To enable stdout capture:
```csharp
var captureService = new StdOutCaptureService();
captureService.StartCapture(); // Opt-in required
```

## Automatic Redaction

The `LogRedactionService` automatically redacts sensitive patterns from log output:

### Patterns Redacted

| Pattern | Example | Redacted As |
|---------|---------|-------------|
| Passwords | `password=secret123` | `password=[REDACTED]` |
| API Keys | `api_key=abc123xyz` | `api_key=[REDACTED]` |
| Tokens | `token=eyJhbGciOiJI...` | `token=[REDACTED]` |
| Bearer Auth | `Bearer eyJhbGciOiJI...` | `Bearer [REDACTED]` |
| Basic Auth | `Basic dXNlcjpwYXNz` | `Basic [REDACTED]` |
| Connection Strings | `Password=secret;` | `Password=[REDACTED];` |
| Credit Cards | `4111111111111111` | `[CARD REDACTED]` |
| SSN | `123-45-6789` | `[SSN REDACTED]` |

### Custom Redaction Patterns

Add custom patterns:
```csharp
var redaction = new LogRedactionService();
redaction.AddPattern(@"CustomSecret=\w+", "CustomSecret=[REDACTED]");
```

## Copy to Clipboard

The debug console provides two copy options:

### Copy (Redacted) - Default
- Applies all redaction rules
- Safe for sharing in bug reports
- Use for most scenarios

### Copy Full (Unredacted)
- Bypasses redaction entirely
- May contain sensitive data
- Use only for local debugging
- Consider adding a confirmation dialog in production

## Best Practices

1. **Never log sensitive data directly**
   ```csharp
   // Bad
   _logger.LogInformation("User logged in with password: {Password}", password);

   // Good
   _logger.LogInformation("User {UserId} logged in successfully", userId);
   ```

2. **Use structured logging**
   ```csharp
   // Structured properties are easier to redact
   _logger.LogInformation("API call to {Endpoint} returned {StatusCode}", endpoint, code);
   ```

3. **Review logs before sharing**
   - Always use "Copy (Redacted)" for external sharing
   - Review output even after redaction

4. **Disable in production builds**
   ```csharp
   #if DEBUG
   services.AddSingleton(loggerProvider);
   #endif
   ```

5. **Keep stdout capture disabled**
   - Only enable when debugging third-party library issues
   - Remember to disable after debugging

## Data Storage

- Logs are stored **in memory only**
- No automatic disk persistence
- Cleared when console window closes
- Maximum entries configurable (default: 10,000)

## Telemetry

AvaloniaAppKit does **not** send any telemetry or analytics data. All logging is local to the user's machine.
