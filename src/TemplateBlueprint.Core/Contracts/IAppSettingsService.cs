namespace TemplateBlueprint.Core.Contracts;

/// <summary>
/// Combined first-run detection and settings persistence service.
/// Settings are stored in platform-appropriate locations:
/// - Windows: %AppData%/[AppName]/settings.json
/// - macOS: ~/Library/Application Support/[AppName]/settings.json
/// - Linux: ~/.config/[AppName]/settings.json
/// </summary>
public interface IAppSettingsService
{
    /// <summary>
    /// Gets whether this is the first time the application is running.
    /// </summary>
    bool IsFirstRun { get; }

    /// <summary>
    /// Marks the first run as complete. Call this after showing welcome UI.
    /// </summary>
    void MarkFirstRunComplete();

    /// <summary>
    /// Gets the settings schema version for migration support.
    /// </summary>
    int SettingsVersion { get; }

    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value, or default if not found.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Sets a setting value by key.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The setting value.</param>
    void Set<T>(string key, T value);

    /// <summary>
    /// Saves all settings to disk.
    /// </summary>
    void Save();

    /// <summary>
    /// Loads settings from disk.
    /// </summary>
    void Load();

    /// <summary>
    /// Gets the full path to the settings file.
    /// </summary>
    string SettingsFilePath { get; }
}

