using System.Collections.ObjectModel;
using AvaloniaTemplateBlueprint.Core.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreChangeType = AvaloniaTemplateBlueprint.Core.Contracts.WorkspaceChangeType;
using CoreEventArgs = AvaloniaTemplateBlueprint.Core.Contracts.WorkspaceChangedEventArgs;

namespace AvaloniaTemplateBlueprint.AppShell.ViewModels;

/// <summary>
/// Manages workspace tabs in the application.
/// </summary>
public partial class WorkspaceManager : ObservableObject, IWorkspaceHost
{
    private readonly List<WorkspaceViewModel> _workspaces = new();

    /// <summary>
    /// Gets or sets the currently active workspace.
    /// </summary>
    [ObservableProperty]
    private WorkspaceViewModel? _activeWorkspace;

    /// <summary>
    /// Gets the collection of open workspaces.
    /// </summary>
    public ObservableCollection<WorkspaceViewModel> Workspaces { get; } = new();

    /// <summary>
    /// Gets or sets the maximum number of workspaces allowed.
    /// </summary>
    public int MaxWorkspaces { get; set; } = 10;

    /// <inheritdoc />
    public event EventHandler<CoreEventArgs>? WorkspaceChanged;

    /// <inheritdoc />
    IReadOnlyList<IWorkspace> IWorkspaceHost.Workspaces =>
        _workspaces.Cast<IWorkspace>().ToList();

    /// <inheritdoc />
    IWorkspace? IWorkspaceHost.ActiveWorkspace => ActiveWorkspace as IWorkspace;

    /// <summary>
    /// Adds a workspace to the manager.
    /// </summary>
    public void AddWorkspace(WorkspaceViewModel workspace)
    {
        if (Workspaces.Count >= MaxWorkspaces)
        {
            throw new InvalidOperationException($"Maximum workspace limit ({MaxWorkspaces}) reached.");
        }

        Workspaces.Add(workspace);
        _workspaces.Add(workspace);
        WorkspaceChanged?.Invoke(this, new CoreEventArgs(null, workspace, CoreChangeType.Added));

        // Activate the newly added workspace
        ActivateWorkspace(workspace);
    }

    /// <inheritdoc />
    void IWorkspaceHost.AddWorkspace(IWorkspace workspace)
    {
        if (workspace is WorkspaceViewModel vm)
        {
            AddWorkspace(vm);
        }
    }

    /// <summary>
    /// Closes a workspace.
    /// </summary>
    public async Task<bool> CloseWorkspaceAsync(WorkspaceViewModel workspace)
    {
        if (!await workspace.CanCloseAsync())
        {
            return false;
        }

        await workspace.OnClosingAsync();

        var index = Workspaces.IndexOf(workspace);
        Workspaces.Remove(workspace);
        _workspaces.Remove(workspace);

        WorkspaceChanged?.Invoke(this, new CoreEventArgs(workspace, null, CoreChangeType.Removed));

        // Activate another workspace if this was active
        if (ActiveWorkspace == workspace)
        {
            var nextIndex = Math.Min(index, Workspaces.Count - 1);
            if (nextIndex >= 0)
            {
                ActivateWorkspace(Workspaces[nextIndex]);
            }
            else
            {
                ActiveWorkspace = null;
            }
        }

        return true;
    }

    /// <inheritdoc />
    void IWorkspaceHost.CloseWorkspace(IWorkspace workspace)
    {
        if (workspace is WorkspaceViewModel vm)
        {
            _ = CloseWorkspaceAsync(vm);
        }
    }

    /// <summary>
    /// Activates a workspace.
    /// </summary>
    public async void ActivateWorkspace(WorkspaceViewModel workspace)
    {
        if (!Workspaces.Contains(workspace))
            return;

        var previous = ActiveWorkspace;

        if (previous != null && previous != workspace)
        {
            await previous.OnDeactivatedAsync();
        }

        ActiveWorkspace = workspace;
        await workspace.OnActivatedAsync();

        WorkspaceChanged?.Invoke(this, new CoreEventArgs(previous, workspace, CoreChangeType.Activated));
    }

    /// <inheritdoc />
    void IWorkspaceHost.ActivateWorkspace(IWorkspace workspace)
    {
        if (workspace is WorkspaceViewModel vm)
        {
            ActivateWorkspace(vm);
        }
    }

    /// <summary>
    /// Checks if any workspace has unsaved changes.
    /// </summary>
    public bool HasUnsavedChanges => Workspaces.Any(w => w.IsDirty);

    /// <summary>
    /// Closes all workspaces, prompting for unsaved changes.
    /// </summary>
    /// <returns>True if all workspaces were closed.</returns>
    public async Task<bool> CloseAllWorkspacesAsync()
    {
        // Close in reverse order (most recently opened first)
        var workspacesToClose = Workspaces.ToList();
        workspacesToClose.Reverse();

        foreach (var workspace in workspacesToClose)
        {
            if (!await CloseWorkspaceAsync(workspace))
            {
                return false; // User cancelled
            }
        }

        return true;
    }

    partial void OnActiveWorkspaceChanged(WorkspaceViewModel? value)
    {
        // Update selection state on all workspaces
        foreach (var ws in Workspaces)
        {
            // If WorkspaceViewModel had an IsActive property, update it here
        }
    }
}
