using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaTemplateBlueprint.AppShell.Services;
using AvaloniaTemplateBlueprint.Core.Contracts;
using AvaloniaTemplateBlueprint.Demo.Services;
using AvaloniaTemplateBlueprint.Demo.ViewModels;
using AvaloniaTemplateBlueprint.Demo.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AvaloniaTemplateBlueprint.Demo;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Avoid duplicate validations from both Avalonia and CommunityToolkit.Mvvm
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }

        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Detect features
        FeatureFlags.DetectFeatures();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging
        var loggerProvider = new ConsoleWindowLoggerProvider();
        services.AddSingleton(loggerProvider);
        services.AddLogging(builder =>
        {
            builder.AddProvider(loggerProvider);
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Core services
        services.AddSingleton<IAppSettingsService, DemoAppSettingsService>();
        services.AddSingleton<IDialogService, DemoDialogService>();
        services.AddSingleton<IRecentFilesService, DemoRecentFilesService>();
        services.AddSingleton<IProjectDirtyTracker, ProjectDirtyTracker>();
        services.AddSingleton<IDataImportService, Core.Services.CsvImportService>();

        // View models
        services.AddTransient<MainWindowViewModel>();
    }
}

/// <summary>
/// Feature flags for optional packages.
/// </summary>
public static class FeatureFlags
{
    /// <summary>
    /// Gets whether Excel import is available.
    /// </summary>
    public static bool ExcelImportAvailable { get; private set; }

    /// <summary>
    /// Detects available features based on loaded assemblies.
    /// </summary>
    public static void DetectFeatures()
    {
        // Check if Excel package is loaded
        ExcelImportAvailable = Type.GetType(
            "AvaloniaTemplateBlueprint.Import.Excel.ExcelImportService, AvaloniaTemplateBlueprint.Import.Excel"
        ) != null;
    }
}
