namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Represents a recently opened file.
/// </summary>
public record RecentFile(string Path, string? DisplayName, DateTime LastOpened);

/// <summary>
/// Tracks recently opened files.
/// </summary>
public interface IRecentFilesService
{
    /// <summary>
    /// Gets the list of recently opened files.
    /// </summary>
    IReadOnlyList<RecentFile> RecentFiles { get; }

    /// <summary>
    /// Adds a file to the recent files list.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="displayName">Optional display name override.</param>
    void AddRecentFile(string path, string? displayName = null);

    /// <summary>
    /// Clears all recent files.
    /// </summary>
    void ClearRecentFiles();

    /// <summary>
    /// Raised when the recent files list changes.
    /// </summary>
    event EventHandler? RecentFilesChanged;
}
