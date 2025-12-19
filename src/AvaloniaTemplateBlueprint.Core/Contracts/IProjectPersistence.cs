namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Project file save/load operations.
/// </summary>
public interface IProjectPersistence
{
    /// <summary>
    /// Saves the current project to the specified path.
    /// </summary>
    /// <param name="path">The file path to save to.</param>
    /// <returns>True if save was successful.</returns>
    Task<bool> SaveAsync(string path);

    /// <summary>
    /// Loads a project from the specified path.
    /// </summary>
    /// <param name="path">The file path to load from.</param>
    /// <returns>True if load was successful.</returns>
    Task<bool> LoadAsync(string path);

    /// <summary>
    /// Gets the current project file path, or null if not saved.
    /// </summary>
    string? CurrentProjectPath { get; }

    /// <summary>
    /// Gets the default file extension for project files (e.g., ".aapk").
    /// </summary>
    string DefaultFileExtension { get; }
}
