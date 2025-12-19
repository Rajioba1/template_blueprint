using System.Text.Json;
using TemplateBlueprint.Core.Contracts;

namespace TemplateBlueprint.Demo.Services;

/// <summary>
/// Demo implementation of app settings service using JSON file storage.
/// </summary>
public class DemoAppSettingsService : IAppSettingsService
{
    private readonly Dictionary<string, JsonElement> _settings = new();
    private readonly string _settingsPath;
    private bool _isFirstRun;
    private const int CurrentVersion = 1;

    public DemoAppSettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "TemplateBlueprint.Demo");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "settings.json");

        Load();
    }

    public bool IsFirstRun => _isFirstRun;

    public int SettingsVersion => CurrentVersion;

    public string SettingsFilePath => _settingsPath;

    public void MarkFirstRunComplete()
    {
        _isFirstRun = false;
        Set("firstRunComplete", true);
        Save();
    }

    public T? Get<T>(string key)
    {
        if (_settings.TryGetValue(key, out var element))
        {
            try
            {
                return element.Deserialize<T>();
            }
            catch
            {
                return default;
            }
        }
        return default;
    }

    public void Set<T>(string key, T value)
    {
        var json = JsonSerializer.SerializeToElement(value);
        _settings[key] = json;
    }

    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently fail - settings are not critical
        }
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (loaded != null)
                {
                    _settings.Clear();
                    foreach (var kvp in loaded)
                    {
                        _settings[kvp.Key] = kvp.Value;
                    }
                }
                _isFirstRun = Get<bool?>("firstRunComplete") != true;
            }
            else
            {
                _isFirstRun = true;
            }
        }
        catch
        {
            _isFirstRun = true;
        }
    }
}

