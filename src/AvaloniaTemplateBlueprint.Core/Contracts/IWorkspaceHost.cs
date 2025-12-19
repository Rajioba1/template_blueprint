namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Event arguments for workspace change events.
/// </summary>
public class WorkspaceChangedEventArgs : EventArgs
{
    public IWorkspace? OldWorkspace { get; }
    public IWorkspace? NewWorkspace { get; }
    public WorkspaceChangeType ChangeType { get; }

    public WorkspaceChangedEventArgs(IWorkspace? oldWorkspace, IWorkspace? newWorkspace, WorkspaceChangeType changeType)
    {
        OldWorkspace = oldWorkspace;
        NewWorkspace = newWorkspace;
        ChangeType = changeType;
    }
}

/// <summary>
/// Type of workspace change.
/// </summary>
public enum WorkspaceChangeType
{
    Added,
    Removed,
    Activated
}

/// <summary>
/// Manages multiple workspaces (tabs) in the application.
/// </summary>
public interface IWorkspaceHost
{
    /// <summary>
    /// Adds a new workspace.
    /// </summary>
    void AddWorkspace(IWorkspace workspace);

    /// <summary>
    /// Closes and removes a workspace.
    /// </summary>
    void CloseWorkspace(IWorkspace workspace);

    /// <summary>
    /// Activates (switches to) a workspace.
    /// </summary>
    void ActivateWorkspace(IWorkspace workspace);

    /// <summary>
    /// Gets all open workspaces.
    /// </summary>
    IReadOnlyList<IWorkspace> Workspaces { get; }

    /// <summary>
    /// Gets the currently active workspace.
    /// </summary>
    IWorkspace? ActiveWorkspace { get; }

    /// <summary>
    /// Gets the maximum number of workspaces allowed.
    /// </summary>
    int MaxWorkspaces { get; }

    /// <summary>
    /// Raised when workspaces change (added, removed, or activated).
    /// </summary>
    event EventHandler<WorkspaceChangedEventArgs>? WorkspaceChanged;
}
