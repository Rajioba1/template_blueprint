namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Tracks whether the project has unsaved changes.
/// </summary>
public interface IProjectDirtyTracker
{
    /// <summary>
    /// Gets whether there are unsaved changes.
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Marks the project as having unsaved changes.
    /// </summary>
    void MarkDirty();

    /// <summary>
    /// Marks the project as saved (no unsaved changes).
    /// </summary>
    void MarkClean();

    /// <summary>
    /// Raised when the dirty state changes.
    /// </summary>
    event EventHandler? DirtyStateChanged;
}
