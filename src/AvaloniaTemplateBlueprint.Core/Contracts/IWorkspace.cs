namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Represents a workspace (tab) in the application.
/// </summary>
public interface IWorkspace
{
    /// <summary>
    /// Gets the display title for this workspace.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets whether this workspace has unsaved changes.
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Gets the view type to display for this workspace.
    /// </summary>
    Type ViewType { get; }

    /// <summary>
    /// Called when the user attempts to close this workspace.
    /// Return false to cancel the close (e.g., for unsaved changes prompt).
    /// </summary>
    /// <returns>True if the workspace can be closed, false to cancel.</returns>
    Task<bool> CanCloseAsync();
}
