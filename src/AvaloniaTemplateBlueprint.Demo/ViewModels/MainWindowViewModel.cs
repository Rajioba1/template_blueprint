using System.Collections.ObjectModel;
using AvaloniaTemplateBlueprint.AppShell.Services;
using AvaloniaTemplateBlueprint.AppShell.ViewModels;
using AvaloniaTemplateBlueprint.AppShell.Windows;
using AvaloniaTemplateBlueprint.Core.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AvaloniaTemplateBlueprint.Demo.ViewModels;

/// <summary>
/// Main window view model demonstrating the template features.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IDialogService _dialogService;
    private readonly IAppSettingsService _settingsService;
    private readonly IRecentFilesService _recentFilesService;
    private readonly IDataImportService _importService;
    private readonly ConsoleWindowLoggerProvider _loggerProvider;
    private DebugConsoleWindow? _consoleWindow;

    /// <summary>
    /// Gets the navigation items for the sidebar.
    /// </summary>
    public ObservableCollection<NavigatorItemViewModel> NavigatorItems { get; } = new();

    /// <summary>
    /// Gets or sets the selected navigation item.
    /// </summary>
    [ObservableProperty]
    private NavigatorItemViewModel? _selectedNavigatorItem;

    /// <summary>
    /// Gets the workspace manager.
    /// </summary>
    public WorkspaceManager WorkspaceManager { get; } = new();

    /// <summary>
    /// Gets or sets the window title.
    /// </summary>
    [ObservableProperty]
    private string _title = "AvaloniaTemplateBlueprint Demo";

    /// <summary>
    /// Gets or sets the status bar text.
    /// </summary>
    [ObservableProperty]
    private string _statusText = "Ready";

    /// <summary>
    /// Gets whether Excel import is available.
    /// </summary>
    public bool ExcelImportAvailable => FeatureFlags.ExcelImportAvailable;

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IDialogService dialogService,
        IAppSettingsService settingsService,
        IRecentFilesService recentFilesService,
        IDataImportService importService,
        ConsoleWindowLoggerProvider loggerProvider)
    {
        _logger = logger;
        _dialogService = dialogService;
        _settingsService = settingsService;
        _recentFilesService = recentFilesService;
        _importService = importService;
        _loggerProvider = loggerProvider;

        InitializeNavigation();

        _logger.LogInformation("Application started");

        // Check first run
        if (_settingsService.IsFirstRun)
        {
            _logger.LogInformation("First run detected - showing welcome experience");
            _settingsService.MarkFirstRunComplete();
        }
    }

    private void InitializeNavigation()
    {
        NavigatorItems.Add(new NavigatorItemViewModel("Welcome", "home"));
        NavigatorItems.Add(new NavigatorItemViewModel("Data Grid", "grid"));

        var settingsGroup = NavigatorItemViewModel.CreateGroup("Settings", "settings",
            new NavigatorItemViewModel("General", "settings"),
            new NavigatorItemViewModel("Appearance", "palette"));
        NavigatorItems.Add(settingsGroup);
    }

    /// <summary>
    /// Shows the debug console window.
    /// </summary>
    [RelayCommand]
    private void ToggleConsole()
    {
        if (_consoleWindow == null)
        {
            _consoleWindow = new DebugConsoleWindow();
            _consoleWindow.Connect(_loggerProvider);
            _consoleWindow.Closed += (s, e) => _consoleWindow = null;
        }

        if (_consoleWindow.IsVisible)
        {
            _consoleWindow.Hide();
        }
        else
        {
            _consoleWindow.Show();
        }
    }

    /// <summary>
    /// Opens a file.
    /// </summary>
    [RelayCommand]
    private async Task OpenFile()
    {
        var extensions = _importService.SupportedExtensions.ToList();
        if (ExcelImportAvailable)
        {
            extensions.AddRange(new[] { ".xlsx", ".xls", ".xlsm" });
        }

        var filters = new[]
        {
            new FileFilter("Data Files", extensions.ToArray()),
            new FileFilter("All Files", new[] { ".*" })
        };

        var path = await _dialogService.ShowOpenFileAsync("Open Data File", filters);
        if (path != null)
        {
            _logger.LogInformation("Opening file: {Path}", path);
            StatusText = $"Loading {System.IO.Path.GetFileName(path)}...";

            try
            {
                var result = await _importService.ImportAsync(path);
                if (result.Success)
                {
                    _recentFilesService.AddRecentFile(path);
                    StatusText = $"Loaded {result.Rows.Count} rows from {System.IO.Path.GetFileName(path)}";
                    _logger.LogInformation("Loaded {Rows} rows, {Cols} columns", result.Rows.Count, result.Columns.Count);
                }
                else
                {
                    StatusText = "Error loading file";
                    await _dialogService.ShowMessageAsync(result.ErrorMessage ?? "Unknown error", "Import Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import file");
                StatusText = "Error loading file";
                await _dialogService.ShowMessageAsync(ex.Message, "Import Error");
            }
        }
    }

    /// <summary>
    /// Shows the about dialog.
    /// </summary>
    [RelayCommand]
    private async Task ShowAbout()
    {
        await _dialogService.ShowMessageAsync(
            "AvaloniaTemplateBlueprint Demo\n\nA template for building Avalonia desktop applications.\n\nVersion 1.0.0",
            "About");
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    [RelayCommand]
    private void Exit()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    partial void OnSelectedNavigatorItemChanged(NavigatorItemViewModel? value)
    {
        if (value != null)
        {
            _logger.LogDebug("Navigation: {Title}", value.Title);
            StatusText = $"Selected: {value.Title}";
        }
    }
}
