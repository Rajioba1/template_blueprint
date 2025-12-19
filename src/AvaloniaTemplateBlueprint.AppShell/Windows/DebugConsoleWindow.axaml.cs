using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using AvaloniaTemplateBlueprint.AppShell.Services;
using AvaloniaTemplateBlueprint.Core.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AvaloniaTemplateBlueprint.AppShell.Windows;

/// <summary>
/// VS Code-style debug console window.
/// </summary>
public partial class DebugConsoleWindow : Window, ILogConsoleSink
{
    private readonly DebugConsoleViewModel _viewModel;

    public DebugConsoleWindow()
    {
        InitializeComponent();
        _viewModel = new DebugConsoleViewModel(this);
        DataContext = _viewModel;
    }

    /// <summary>
    /// Connects the console to a logger provider.
    /// </summary>
    public void Connect(ConsoleWindowLoggerProvider provider)
    {
        _viewModel.Connect(provider);
    }

    /// <inheritdoc />
    public void Log(LogLevel level, string message)
    {
        _viewModel.AddEntry(new LogEntry(DateTime.Now, level, "App", message));
    }

    /// <inheritdoc />
    public void LogException(Exception ex, string context)
    {
        _viewModel.AddEntry(new LogEntry(DateTime.Now, LogLevel.Error, context, ex.Message, ex));
    }

    /// <inheritdoc />
    void ILogConsoleSink.Show() => Show();

    /// <inheritdoc />
    void ILogConsoleSink.Hide() => Hide();

    /// <inheritdoc />
    void ILogConsoleSink.Clear() => _viewModel.Clear();

    /// <inheritdoc />
    bool ILogConsoleSink.IsVisible => IsVisible;

    /// <inheritdoc />
    public string GetLogs(bool redacted) => _viewModel.GetLogsAsText(redacted);
}

/// <summary>
/// View model for the debug console window.
/// </summary>
public partial class DebugConsoleViewModel : ObservableObject
{
    private readonly DebugConsoleWindow _window;
    private ConsoleWindowLoggerProvider? _provider;
    private LogLevel _minLevel = LogLevel.Debug;

    /// <summary>
    /// Gets all log entries.
    /// </summary>
    public ObservableCollection<LogEntry> Entries { get; } = new();

    /// <summary>
    /// Gets the filtered log entries.
    /// </summary>
    public ObservableCollection<LogEntry> FilteredEntries { get; } = new();

    /// <summary>
    /// Gets or sets whether to auto-scroll to new entries.
    /// </summary>
    [ObservableProperty]
    private bool _autoScroll = true;

    /// <summary>
    /// Gets or sets whether stdout capture is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _isStdOutCaptureEnabled;

    /// <summary>
    /// Gets the stdout capture status text.
    /// </summary>
    public string StdOutCaptureStatus => IsStdOutCaptureEnabled ? "⚠ StdOut Capture ON" : "";

    /// <summary>
    /// Gets the total entry count.
    /// </summary>
    public int EntryCount => Entries.Count;

    /// <summary>
    /// Gets the filtered entry count.
    /// </summary>
    public int FilteredCount => FilteredEntries.Count;

    public DebugConsoleViewModel(DebugConsoleWindow window)
    {
        _window = window;
    }

    /// <summary>
    /// Connects to a logger provider.
    /// </summary>
    public void Connect(ConsoleWindowLoggerProvider provider)
    {
        if (_provider != null)
        {
            _provider.LogEntryAdded -= OnLogEntryAdded;
        }

        _provider = provider;
        _provider.LogEntryAdded += OnLogEntryAdded;

        // Load existing entries
        foreach (var entry in _provider.GetEntries())
        {
            AddEntryInternal(entry);
        }
    }

    private void OnLogEntryAdded(object? sender, LogEntryEventArgs e)
    {
        // Marshal to UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() => AddEntryInternal(e.Entry));
    }

    internal void AddEntry(LogEntry entry)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => AddEntryInternal(entry));
    }

    private void AddEntryInternal(LogEntry entry)
    {
        Entries.Add(entry);

        if (entry.Level >= _minLevel)
        {
            FilteredEntries.Add(entry);
        }

        OnPropertyChanged(nameof(EntryCount));
        OnPropertyChanged(nameof(FilteredCount));

        if (AutoScroll)
        {
            // Auto-scroll handled by ScrollViewer in XAML
        }
    }

    /// <summary>
    /// Clears all entries.
    /// </summary>
    [RelayCommand]
    public void Clear()
    {
        Entries.Clear();
        FilteredEntries.Clear();
        _provider?.Clear();
        OnPropertyChanged(nameof(EntryCount));
        OnPropertyChanged(nameof(FilteredCount));
    }

    /// <summary>
    /// Copies logs to clipboard with redaction.
    /// </summary>
    [RelayCommand]
    public async Task CopyRedacted()
    {
        var text = GetLogsAsText(redacted: true);
        await CopyToClipboardAsync(text);
    }

    /// <summary>
    /// Copies full logs to clipboard (may contain sensitive data).
    /// </summary>
    [RelayCommand]
    public async Task CopyFull()
    {
        var text = GetLogsAsText(redacted: false);
        await CopyToClipboardAsync(text);
    }

    internal string GetLogsAsText(bool redacted)
    {
        if (_provider != null)
        {
            return _provider.GetLogsAsText(redacted);
        }

        return string.Join(Environment.NewLine, Entries.Select(e => e.FormattedMessage));
    }

    private async Task CopyToClipboardAsync(string text)
    {
        var clipboard = TopLevel.GetTopLevel(_window)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}
