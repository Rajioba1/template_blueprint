using AvaloniaTemplateBlueprint.Core.Contracts;

namespace AvaloniaTemplateBlueprint.AppShell.Services;

/// <summary>
/// Tracks the dirty (unsaved changes) state of a project.
/// Supports both global and per-workspace dirty tracking.
/// </summary>
public class ProjectDirtyTracker : IProjectDirtyTracker
{
    private readonly Dictionary<Guid, bool> _workspaceDirtyStates = new();
    private bool _globalDirty;

    /// <inheritdoc />
    public bool IsDirty => _globalDirty || _workspaceDirtyStates.Values.Any(v => v);

    /// <inheritdoc />
    public event EventHandler? DirtyStateChanged;

    /// <summary>
    /// Gets whether a specific workspace has unsaved changes.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <returns>True if the workspace has unsaved changes.</returns>
    public bool IsWorkspaceDirty(Guid workspaceId)
    {
        return _workspaceDirtyStates.TryGetValue(workspaceId, out var dirty) && dirty;
    }

    /// <inheritdoc />
    public void MarkDirty()
    {
        if (!_globalDirty)
        {
            _globalDirty = true;
            DirtyStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Marks a specific workspace as dirty.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    public void MarkWorkspaceDirty(Guid workspaceId)
    {
        var wasDirty = IsDirty;
        _workspaceDirtyStates[workspaceId] = true;

        if (!wasDirty && IsDirty)
        {
            DirtyStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public void MarkClean()
    {
        var wasDirty = IsDirty;
        _globalDirty = false;
        _workspaceDirtyStates.Clear();

        if (wasDirty)
        {
            DirtyStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Marks a specific workspace as clean.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    public void MarkWorkspaceClean(Guid workspaceId)
    {
        var wasDirty = IsDirty;

        if (_workspaceDirtyStates.ContainsKey(workspaceId))
        {
            _workspaceDirtyStates[workspaceId] = false;
        }

        if (wasDirty && !IsDirty)
        {
            DirtyStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Removes a workspace from tracking.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    public void RemoveWorkspace(Guid workspaceId)
    {
        var wasDirty = IsDirty;
        _workspaceDirtyStates.Remove(workspaceId);

        if (wasDirty && !IsDirty)
        {
            DirtyStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
