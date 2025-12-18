using System.Text.Json;
using AvaloniaAppKit.Core.Contracts;

namespace AvaloniaAppKit.Demo.Services;

/// <summary>
/// Demo implementation of recent files service using JSON file storage.
/// </summary>
public class DemoRecentFilesService : IRecentFilesService
{
    private readonly List<RecentFile> _recentFiles = new();
    private readonly string _recentFilesPath;
    private const int MaxRecentFiles = 10;

    public DemoRecentFilesService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "AvaloniaAppKit.Demo");
        Directory.CreateDirectory(appFolder);
        _recentFilesPath = Path.Combine(appFolder, "recent_files.json");

        Load();
    }

    public IReadOnlyList<RecentFile> RecentFiles => _recentFiles.AsReadOnly();

    public event EventHandler? RecentFilesChanged;

    public void AddRecentFile(string path, string? displayName = null)
    {
        // Remove existing entry with same path
        _recentFiles.RemoveAll(f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase));

        // Add to front
        var fileName = displayName ?? Path.GetFileName(path);
        _recentFiles.Insert(0, new RecentFile(path, fileName, DateTime.Now));

        // Trim to max
        while (_recentFiles.Count > MaxRecentFiles)
        {
            _recentFiles.RemoveAt(_recentFiles.Count - 1);
        }

        Save();
        RecentFilesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearRecentFiles()
    {
        _recentFiles.Clear();
        Save();
        RecentFilesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_recentFiles, options);
            File.WriteAllText(_recentFilesPath, json);
        }
        catch
        {
            // Silently fail
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_recentFilesPath))
            {
                var json = File.ReadAllText(_recentFilesPath);
                var loaded = JsonSerializer.Deserialize<List<RecentFile>>(json);
                if (loaded != null)
                {
                    _recentFiles.Clear();
                    _recentFiles.AddRange(loaded);
                }
            }
        }
        catch
        {
            // Start fresh
        }
    }
}
